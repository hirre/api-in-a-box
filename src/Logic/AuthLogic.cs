using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiInABox.Logic
{
    public class AuthLogic
    {
        public async Task<string> AuthUser(DatabaseContext dbContext, Secret secret, AuthUserRequest authUserRequestObj)
        {
            var loadedUser = await dbContext.Users
                                .Include(x => x.Roles)
                                .FirstOrDefaultAsync(x => x.UserName == authUserRequestObj.UserName && x.Activated);

            if (loadedUser == null)
                throw new AccessDeniedException();

            if (!BCrypt.Net.BCrypt.Verify(authUserRequestObj.Password, loadedUser.Password))
                throw new AccessDeniedException();

            var roles = "";
            foreach (var r in loadedUser.Roles)
            {
                roles += $"{r.Name.ToLower()},";
            }
            roles = roles.Substring(0, roles.Length - 1);

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "" + loadedUser.UserName),
                new Claim(ClaimTypes.Role, roles)
            };

            var token = TokenFactory.Generate(secret.Token.Key, secret.Token.Issuer, secret.Token.Audience,
                        DateTime.Now.AddHours(1), claims);

            return token;
        }

        public async Task<string> AuthApi(DatabaseContext dbContext, Secret secret, AuthApiRequest authApiKeyRequestObj)
        {
            var loadedApiKey = await dbContext.ApiKeys
                            .FirstOrDefaultAsync(x => x.Name == authApiKeyRequestObj.Name);

            if (loadedApiKey == null)
                throw new AccessDeniedException();            

            if (!BCrypt.Net.BCrypt.Verify(authApiKeyRequestObj.Key, loadedApiKey.Key))
                throw new AccessDeniedException();

            if (Instant.FromDateTimeUtc(DateTime.UtcNow) > loadedApiKey.ExpirationDate)
                throw new AccessDeniedException("API key has expired");

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, loadedApiKey.Name),
                new Claim(ClaimTypes.Expiration, loadedApiKey.ExpirationDate.ToString()),
                new Claim(ClaimTypes.Role, "api")
            };

            var token = TokenFactory.Generate(secret.Token.Key, secret.Token.Issuer, secret.Token.Audience,
                        DateTime.Now.AddDays(1), claims);

            return token;
        }
    }
}
