using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<string> GenerateToken()
        {
            var myTestSecret = "thisismysecret123456789123456789"; // String needs to be atleast 32 in length
            var myTestSecretSecString = new SecureString();

            foreach (var c in myTestSecret.ToCharArray())
            {
                myTestSecretSecString.AppendChar(c);
            }

            var myIssuer = "http://test.com";
            var myAudience = "http://testaudience.com";

            var claimType = "TestType";
            var claims = new Claim[] { new Claim(claimType, "Hello"), 
                            new Claim("AnotherType", "Something") };

            var token = TokenFactory.Generate(myTestSecretSecString, myIssuer, myAudience, 
                        DateTime.Now.AddDays(1), 
                        new Claim[] { new Claim("test", "testvalue")});

            return await Task.FromResult(token);
        }
    }
}
