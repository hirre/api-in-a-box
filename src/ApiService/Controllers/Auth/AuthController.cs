using System.Threading.Tasks;
using ApiService.Contexts;
using ApiService.Logic;
using ApiService.Models;
using ApiService.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Auth;

namespace ApiService.Controllers
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
        [Route("AuthUser")]
        public async Task<string> AuthUser([FromBody] User user)
        {
            return await _authLogic.AuthUser(_dbContext, _secret, user);
        }

        [HttpPost]
        [Route("AuthApi")]
        public async Task<string> AuthApi([FromBody] ApiKey apiKey)
        {
            return await _authLogic.AuthApi(_dbContext, _secret, apiKey);
        }
    }
}
