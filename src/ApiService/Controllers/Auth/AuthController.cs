using System;
using System.Threading.Tasks;
using ApiInABox.Contexts;
using ApiInABox.Logic;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiInABox.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly DatabaseContext _dbContext;
        private readonly Secret _secret;
        private readonly AuthLogic _authLogic;

        public AuthController(DatabaseContext dbContext, Secret secret, AuthLogic authLogic,
            ILogger<AuthController> logger)
        {
            _secret = secret;
            _logger = logger;
            _dbContext = dbContext;
            _authLogic = authLogic;
        }

        [HttpPost]
        [Route("User")]
        public async Task AuthUser([FromBody] AuthUserRequest authUserRequestObj)
        {
            var token = await _authLogic.AuthUser(_dbContext, _secret, authUserRequestObj);

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };
            
            Response.Cookies.Append("Auth", token, options);
        }

        [HttpPost]
        [Route("Api")]
        public async Task AuthApi([FromBody] AuthApiRequest authApiKeyRequestObj)
        {
            var token = await _authLogic.AuthApi(_dbContext, _secret, authApiKeyRequestObj);

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            Response.Cookies.Append("Auth", token, options);
        }
    }
}
