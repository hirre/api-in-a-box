using System.Threading.Tasks;
using ApiInABox.Contexts;
using ApiInABox.Logic;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
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
        public async Task<string> AuthUser([FromBody] AuthUserRequest authUserRequestObj)
        {
            return await _authLogic.AuthUser(_dbContext, _secret, authUserRequestObj);
        }

        [HttpPost]
        [Route("Api")]
        public async Task<string> AuthApi([FromBody] AuthApiRequest authApiKeyRequestObj)
        {
            return await _authLogic.AuthApi(_dbContext, _secret, authApiKeyRequestObj);
        }
    }
}
