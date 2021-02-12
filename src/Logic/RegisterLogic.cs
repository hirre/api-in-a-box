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

namespace ApiInABox.Logic
{
    public class RegisterLogic
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public RegisterLogic(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<User> CreateUser(DatabaseContext dbContext, RegisterUserRequest regUserObj)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.UserName == regUserObj.UserName || x.ActivationEmail == regUserObj.ActivationEmail);

            if (loadedUser != null)
                throw new ObjectExistsException("Username/e-mail already exists");

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

        public async Task<User> ResendActivationEmail(DatabaseContext dbContext, string activationEmail)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.ActivationEmail == activationEmail && !x.Activated);

            if (loadedUser == null)
                throw new ObjectNotExistsException("E-mail doesn't exist");

            loadedUser.TemporarySecret = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

            var res = await dbContext.SaveChangesAsync();

            if (res == 0)
                throw new FailedSaveException("Failed saving user");

            var msg = $"<h2 style=\"font-family:verdana;\">Registration confirmation</h2>" +
                $"<p style=\"font-family:verdana;\">" + $"Username: {loadedUser.UserName}<br><br>" +
                $"Please click on <a href=\"{_configuration["DomainNameURL"]}/activation?u={loadedUser.TemporarySecret}\">activate</a> " +
                $"to complete your registration.<br><br>Thank you!</p>";

            await _emailService.SendAsync(loadedUser.ActivationEmail, "Activate account", msg, true);

            return loadedUser;
        }

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
