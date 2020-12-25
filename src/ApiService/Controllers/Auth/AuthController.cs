using System.Threading.Tasks;
using ApiService.Contexts;
using ApiService.Logic;
using ApiService.Models;
using ApiService.Models.Auth;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<string> Auth([FromBody] User user)
        {
            return await _authLogic.Auth(_dbContext, _secret, user);
        }
    }
}
