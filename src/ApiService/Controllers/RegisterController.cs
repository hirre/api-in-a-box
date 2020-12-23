using ApiService.Contexts;
using ApiService.Exceptions;
using ApiService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Threading.Tasks;

namespace ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly DatabaseContext _dbContext;

        public RegisterController(ILogger<RegisterController> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<User> Post([FromBody] User user)
        {
            var loadedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName.Equals(user.UserName));

            if (loadedUser != null)
                throw new HttpException("User already exists", 409);

            var newUser = new User
            {
                UserName = user.UserName,
            };

            newUser.Roles.Add(new Role() { Name = "User" });
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, BCrypt.Net.BCrypt.GenerateSalt());

            await _dbContext.Users.AddAsync(newUser);
            var res = await _dbContext.SaveChangesAsync();

            if (res == 0)
                throw new HttpException("Failed saving user", 500);

            return newUser;
        }
    }
}
