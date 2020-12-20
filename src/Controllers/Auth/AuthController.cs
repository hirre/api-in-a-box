using System;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiService.Contexts;
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

        public AuthController(ILogger<AuthController> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<string> Auth([FromBody] User user)
        {
            var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName);

            if (loadedUser == null)
                throw new Exception("Access denied.");

            if (!BCrypt.Net.BCrypt.Verify(user.Password, loadedUser.Password))
                throw new Exception("Access denied.");

            //BCrypt.Net.BCrypt.HashPassword()
            //BCrypt.Net.BCrypt.Verify()

            var myTestSecret = "thisismysecret123456789123456789"; // String needs to be atleast 32 in length
            var myTestSecretSecString = new SecureString();

            foreach (var c in myTestSecret.ToCharArray())
            {
                myTestSecretSecString.AppendChar(c);
            }

            var myIssuer = "http://test.com";
            var myAudience = "http://testaudience.com";

            var claimType = "TestType";
            var claims = new Claim[] 
            { 
                new Claim(claimType, "Hello"), 
                new Claim("AnotherType", "Something") 
            };

            var token = TokenFactory.Generate(myTestSecretSecString, myIssuer, myAudience, 
                        DateTime.Now.AddDays(1), claims);

            return await Task.FromResult(token);
        }
    }
}
