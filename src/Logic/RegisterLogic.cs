/**
	Copyright 2021 Hirad Asadi (API in a Box)

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Models;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NETCore.MailKit.Core;
using NodaTime;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace ApiInABox.Logic
{
    public class RegisterLogic
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterLogic(IEmailService emailService, IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _emailService = emailService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        ///     Created the user and checks the Captcha.
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="regUserObj">User request object</param>
        /// <returns>The created user or HTTP error code.</returns>
        public async Task<User> CreateUser(DatabaseContext dbContext, RegisterUserRequest regUserObj)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.UserName == regUserObj.UserName || x.ActivationEmail == regUserObj.ActivationEmail);

            if (loadedUser != null)
                throw new ObjectExistsException("Username/e-mail already exists");

            if (string.IsNullOrEmpty(regUserObj.ReCaptcha))
                throw new AccessDeniedException("ReCaptcha not set!");

            // Check reCaptcha
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                new StringContent($"secret=${_configuration["ReCaptchaServerKey"]}&response=${regUserObj.ReCaptcha}",
                    Encoding.UTF8, "application/x-www-form-urlencoded"));

            if (!response.IsSuccessStatusCode)
                throw new AccessDeniedException("ReCaptcha failed!");

            var newUser = new User
            {
                UserName = regUserObj.UserName,
                ActivationEmail = regUserObj.ActivationEmail,
                TemporarySecret = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "")
            };

            newUser.Roles.Add(new Role() { Name = "user" });
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(regUserObj.Password, BCrypt.Net.BCrypt.GenerateSalt());

            await dbContext.Users.AddAsync(newUser);
            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new FailedSaveException("Failed saving user");

            var msg = $"<h2 style=\"font-family:verdana;\">Registration confirmation</h2>" +
                $"<p style=\"font-family:verdana;\">" + $"Username: {newUser.UserName}<br><br>" +
                $"Please click on <a href=\"{_configuration["DomainNameURL"]}/activation?u={newUser.TemporarySecret}\">activate</a> " +
                $"to complete your registration.<br><br>Thank you!</p>";

            await _emailService.SendAsync(newUser.ActivationEmail, "Activate account", msg, true);

            return newUser;
        }

        /// <summary>
        ///     Reset password.
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="rpr">Request object</param>
        /// <returns></returns>
        public async Task<User> ResetPassword(DatabaseContext dbContext, ResetPasswordRequest rpr)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.TemporarySecret == rpr.TemporaryCode);

            if (loadedUser == null)
                throw new ObjectNotExistsException("User doesn't exist");

            loadedUser.TemporarySecret = null;
            loadedUser.Password = BCrypt.Net.BCrypt.HashPassword(rpr.NewPassword,
                BCrypt.Net.BCrypt.GenerateSalt());

            await dbContext.SaveChangesAsync();

            return loadedUser;
        }

        /// <summary>
        ///     Resends activation e-mail.
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="activationEmail">Activation e-mail address</param>
        /// <returns>The created user or HTTP error code.</returns>
        public async Task<User> ResendActivationEmail(DatabaseContext dbContext, string activationEmail)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.ActivationEmail == activationEmail);

            if (loadedUser == null)
                throw new ObjectNotExistsException("User doesn't exist");

            var msg = "";
            var title = "";

            loadedUser.TemporarySecret = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new FailedSaveException("Failed saving user");

            if (!loadedUser.Activated)
            {
                msg = $"<h2 style=\"font-family:verdana;\">Registration confirmation</h2>" +
                    $"<p style=\"font-family:verdana;\">" + $"Username: {loadedUser.UserName}<br><br>" +
                    $"Please click on <a href=\"{_configuration["DomainNameURL"]}/activation?u={loadedUser.TemporarySecret}\">activate</a> " +
                    $"to complete your registration.<br><br>Thank you!</p>";

                title = "Activate account";
            }
            else // Reset password
            {
                msg = $"<h2 style=\"font-family:verdana;\">Reset password</h2>" +
                    $"<p style=\"font-family:verdana;\">" + $"Username: {loadedUser.UserName}<br><br>" +
                    $"Please click <a href=\"{_configuration["DomainNameURL"]}/resetpassword?u={loadedUser.TemporarySecret}\">here</a> " +
                    $"to reset your password.<br><br>Thank you!</p>";

                title = "Reset password";
            }

            await _emailService.SendAsync(loadedUser.ActivationEmail, title, msg, true);

            return loadedUser;
        }

        /// <summary>
        ///     Activates a user.
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="temporarySecret">Temporary secret code</param>
        /// <returns>The created user or HTTP error code.</returns>
        public async Task<User> ActivateUser(DatabaseContext dbContext, string temporarySecret)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.TemporarySecret.Equals(temporarySecret));

            if (loadedUser == null)
                throw new ObjectNotExistsException("Activation code doesn't exist");

            if ((Instant.FromDateTimeUtc(DateTime.UtcNow) - loadedUser.UpdatedDate).Hours >= 24)
                throw new BadRequestException("Activation period (24 hours) has passed, request a new activation");

            loadedUser.Activated = true;
            loadedUser.TemporarySecret = null;

            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new FailedSaveException("Failed saving user");

            return loadedUser;
        }

        /// <summary>
        ///     Creates an API key.
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="regApiKeyObj">Register API key requst object</param>
        /// <returns>The API key or HTTP error code</returns>
        public async Task<ApiKey> CreateApiKey(DatabaseContext dbContext, RegisterApiKeyRequest regApiKeyObj)
        {
            var loadedApiKey = await dbContext.ApiKeys
                .FirstOrDefaultAsync(x => x.Name == regApiKeyObj.Name);

            if (loadedApiKey != null)
                throw new ObjectExistsException("API key name already exists");

            var apiKeyStr = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

            var newApiKey = new ApiKey
            {
                Name = regApiKeyObj.Name,
                Key = BCrypt.Net.BCrypt.HashPassword(apiKeyStr, BCrypt.Net.BCrypt.GenerateSalt()),
                ExpirationDate = Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1))
            };

            await dbContext.ApiKeys.AddAsync(newApiKey);
            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new FailedSaveException("Failed saving API key");

            // Rewrite original un-hashed API key to return value so that the user can save it
            newApiKey.Key = apiKeyStr;

            return newApiKey;

        }
    }
}
