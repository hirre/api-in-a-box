using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiService.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthTestController : ControllerBase
    {
        private readonly ILogger<AuthTestController> _logger;

        public AuthTestController(ILogger<AuthTestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public bool Get()
        {
            return true;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [Route("TestRole")]
        public bool TestRole()
        {
            return true;
        }
    }
}
