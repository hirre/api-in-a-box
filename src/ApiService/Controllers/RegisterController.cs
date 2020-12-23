using ApiService.Contexts;
using ApiService.Logic;
using ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApiService.Controllers
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
        public async Task<User> Post([FromBody] User user)
        {
            return await _registerLogic.CreateUser(_dbContext, user);
        }
    }
}
