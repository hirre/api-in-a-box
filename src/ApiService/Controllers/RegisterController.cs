/**
	Copyright 2021 Hirad Asadi (API in a Box)

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

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

        [HttpGet]
        [Route("ActivateUser/{temporarySecret}")]
        public async Task<User> CreateUser(string temporarySecret)
        {
            return await _registerLogic.ActivateUser(_dbContext, temporarySecret);
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<User> ResetPassword([FromBody] ResetPasswordRequest rpr)
        {
            return await _registerLogic.ResetPassword(_dbContext, rpr);
        }

        [HttpGet]
        [Route("ResendActivationEmail/{activationEmail}")]
        public async Task<User> ResendActivationEmail(string activationEmail)
        {
            return await _registerLogic.ResendActivationEmail(_dbContext, activationEmail);
        }

        [HttpPost]
        [Route("ApiKey")]
        [Authorize(Roles = "user")]
        public async Task<ApiKey> CreateApiKey([FromBody] RegisterApiKeyRequest apiKey)
        {
            return await _registerLogic.CreateApiKey(_dbContext, apiKey);
        }
    }
}
