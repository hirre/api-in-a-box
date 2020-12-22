using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiService.Contexts;
using ApiService.Exceptions;
using ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly DatabaseContext _dbContext;
        private readonly Secret _secret;

        public AuthController(Secret secret, ILogger<AuthController> logger, DatabaseContext dbContext)
        {
            _secret = secret;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<string> Auth([FromBody] User user)
        {
            var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName);

            if (loadedUser == null)
                throw new HttpException("Access denied.", 403);

            if (!BCrypt.Net.BCrypt.Verify(user.Password, loadedUser.Password))
                throw new HttpException("Access denied.", 403);
                        
            var claims = new Claim[] 
            {
                new Claim("id", "" + loadedUser.Id),
                new Claim(ClaimTypes.Name, loadedUser.UserName) 
            };

            var token = TokenFactory.Generate(_secret.Token.Key, _secret.Token.Issuer, _secret.Token.Issuer, 
                        DateTime.Now.AddHours(1), claims);

            return await Task.FromResult(token);
        }
    }
}
