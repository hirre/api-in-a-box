﻿using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Models;
using ApiInABox.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiInABox.Logic
{
    public class AuthLogic
    {
        public async Task<string> AuthUser(DatabaseContext dbContext, Secret secret, User user)
        {
            var loadedUser = await dbContext.Users
                                .Include(x => x.Roles)
                                .FirstOrDefaultAsync(x => x.UserName == user.UserName && x.Activated);

            if (loadedUser == null)
                throw new AccessDeniedException();

            if (!BCrypt.Net.BCrypt.Verify(user.Password, loadedUser.Password))
                throw new AccessDeniedException();

            var roles = "";
            foreach (var r in loadedUser.Roles)
            {
                roles += $"{r.Name.ToLower()},";
            }
            roles = roles.Substring(0, roles.Length - 1);

            var claims = new Claim[]
            {
                new Claim("id", "" + loadedUser.Id),
                new Claim(ClaimTypes.Role, roles)
            };

            var token = TokenFactory.Generate(secret.Token.Key, secret.Token.Issuer, secret.Token.Audience,
                        DateTime.Now.AddHours(1), claims);

            return token;
        }

        public async Task<string> AuthApi(DatabaseContext dbContext, Secret secret, ApiKey apiKey)
        {
            var loadedApiKey = await dbContext.ApiKeys
                            .FirstOrDefaultAsync<ApiKey>(x => x.Name == apiKey.Name);

            if (loadedApiKey == null)
                throw new AccessDeniedException();

            if (!BCrypt.Net.BCrypt.Verify(apiKey.Key, loadedApiKey.Key))
                throw new AccessDeniedException();

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Authentication, loadedApiKey.Name),
                new Claim(ClaimTypes.Expiration, loadedApiKey.ExpirationDate.ToString()),
                new Claim(ClaimTypes.Role, "api")
            };

            var token = TokenFactory.Generate(secret.Token.Key, secret.Token.Issuer, secret.Token.Audience,
                        DateTime.Now.AddDays(1), claims);

            return token;
        }
    }
}
