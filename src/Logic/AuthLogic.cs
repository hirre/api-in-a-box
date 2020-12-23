using ApiService.Contexts;
using ApiService.Exceptions;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiService.Logic
{
    public class AuthLogic
    {
        public async Task<string> Auth(DatabaseContext dbContext, Secret secret, User user)
        {
            var loadedUser = await dbContext.Users
                                .Include(x => x.Roles)
                                .FirstOrDefaultAsync(x => x.UserName == user.UserName);

            if (loadedUser == null)
                throw new HttpException("Access denied", 403);

            if (!BCrypt.Net.BCrypt.Verify(user.Password, loadedUser.Password))
                throw new HttpException("Access denied", 403);

            var roles = "";
            foreach (var r in loadedUser.Roles)
            {
                roles += $"{r.Name},";
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
    }
}
