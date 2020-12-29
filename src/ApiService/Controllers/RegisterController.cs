using ApiInABox.Contexts;
using ApiInABox.Logic;
using ApiInABox.Models;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApiInABox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly DatabaseContext _dbContext;
        private readonly RegisterLogic _registerLogic;

        public RegisterController(DatabaseContext dbContext, RegisterLogic registerLogic, 
            ILogger<RegisterController> logger)
        {
            _registerLogic = registerLogic;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("User")]
        public async Task<User> CreateUser([FromBody] RegisterUserRequest regUserObj)
        {
            return await _registerLogic.CreateUser(_dbContext, regUserObj);
        }

        [HttpPost]
        [Route("ActivateUser")]
        public async Task<User> CreateUser([FromQuery] ActivateUserRequest activateUserObj)
        {
            return await _registerLogic.ActivateUser(_dbContext, activateUserObj);
        }

        [HttpPost]
        [Route("ApiKey")]
        [Authorize]
        public async Task<ApiKey> CreateApiKey([FromBody] RegisterApiKeyRequest apiKey)
        {
            return await _registerLogic.CreateApiKey(_dbContext, apiKey);
        }
    }
}
