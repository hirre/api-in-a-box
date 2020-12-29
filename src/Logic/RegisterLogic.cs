using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Models;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ApiInABox.Logic
{
    public class RegisterLogic
    {
        public async Task<User> CreateUser(DatabaseContext dbContext, RegisterUserRequest regUserObj)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.UserName == regUserObj.UserName);

            if (loadedUser != null)
                throw new ObjectExistsException("User already exists");

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

            return newUser;
        }

        public async Task<User> ActivateUser(DatabaseContext dbContext, ActivateUserRequest activateUserObj)
        {
            var loadedUser = await dbContext.Users
                .FirstOrDefaultAsync(x => x.TemporarySecret.Equals(activateUserObj.U));

            if (loadedUser == null)
                throw new ObjectNotExistsException("Activation code doesn't exist");

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
                ExpirationDate = DateTimeOffset.UtcNow.AddYears(1)
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
