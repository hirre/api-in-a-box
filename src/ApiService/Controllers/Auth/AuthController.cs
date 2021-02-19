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

using System;
using System.Threading.Tasks;
using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Logic;
using ApiInABox.Models.Auth;
using ApiInABox.Models.RequestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache _cache;

        public AuthController(DatabaseContext dbContext, IDistributedCache cache,
            Secret secret, AuthLogic authLogic,
            ILogger<AuthController> logger)
        {
            _cache = cache;
            _secret = secret;
            _logger = logger;
            _dbContext = dbContext;
            _authLogic = authLogic;
        }

        [HttpPost]
        [Route("User")]
        public async Task AuthUser([FromBody] AuthUserRequest authUserRequestObj)
        {
            var accessToken = await _authLogic.AuthUser(_dbContext, _secret, authUserRequestObj);

            var accessTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddHours(1),
                IsEssential = true
            };

#if DEBUG
            accessTokenOptions.SameSite = SameSiteMode.None;
#else
            accessTokenOptions.SameSite = SameSiteMode.Strict;
#endif

            var refreshTokenOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(1),
                IsEssential = true,
                Secure = true
            };

#if DEBUG
            refreshTokenOptions.SameSite = SameSiteMode.None;
#else
            refreshTokenOptions.SameSite = SameSiteMode.Strict;
#endif

            var refreshToken = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

            // Set access token and refresh token in cookies
            Response.Cookies.Append("access_token", accessToken, accessTokenOptions);
            Response.Cookies.Append("refresh_token", refreshToken, refreshTokenOptions);
            await _cache.SetStringAsync("Refresh:" + refreshToken, accessToken,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });
        }

        [HttpPost]
        [Route("User/Refresh")]
        public async Task<IActionResult> RefreshAuth()
        {
            if (Request.Cookies.ContainsKey("refresh_token"))
            {
                var refreshToken = Request.Cookies["refresh_token"];
                var refreshTokenKey = "Refresh:" + refreshToken;
                var accessToken = await _cache.GetStringAsync(refreshTokenKey);

                if (!string.IsNullOrEmpty(await _cache.GetStringAsync(refreshToken)))
                {
                    // Token destructed (logged out etc)
                    throw new AccessDeniedException("Refresh token has been invalidated.");
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    if (!string.IsNullOrEmpty(await _cache.GetStringAsync(accessToken)))
                    {
                        // Token destructed (logged out etc)
                        throw new AccessDeniedException("Access token has been invalidated.");
                    }

                    var accessTokenOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddHours(1),
                        IsEssential = true
                    };
#if DEBUG
                    accessTokenOptions.SameSite = SameSiteMode.None;
#else
                    accessTokenOptions.SameSite = SameSiteMode.Strict;
#endif

                    var refreshTokenOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(1),
                        IsEssential = true,
                        Secure = true
                    };

#if DEBUG
                    refreshTokenOptions.SameSite = SameSiteMode.None;
#else
                    refreshTokenOptions.SameSite = SameSiteMode.Strict;
#endif

                    var newRefreshToken = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

                    // Remove old token from cache
                    await _cache.RemoveAsync(refreshTokenKey);

                    // Mark old token as disposed
                    await _cache.SetStringAsync(refreshToken, "X");

                    // Add new refresh token
                    await _cache.SetStringAsync("Refresh:" + newRefreshToken, accessToken);

                    // Set access token and refresh token in cookies
                    Response.Cookies.Append("access_token", accessToken, accessTokenOptions);
                    Response.Cookies.Append("refresh_token", newRefreshToken, refreshTokenOptions);

                    return Ok();
                }
            }

            throw new AccessDeniedException();
        }

        [HttpPost]
        [Route("Api")]
        public async Task AuthApi([FromBody] AuthApiRequest authApiKeyRequestObj)
        {
            var token = await _authLogic.AuthApi(_dbContext, _secret, authApiKeyRequestObj);

            var accessTokenOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(1),
                IsEssential = true
            };

#if DEBUG
            accessTokenOptions.SameSite = SameSiteMode.None;
#else
            accessTokenOptions.SameSite = SameSiteMode.Strict;
#endif

            // Set access token in cookie
            Response.Cookies.Append("access_token", token, accessTokenOptions);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.ContainsKey("access_token"))
            {
                var accessToken = Request.Cookies["access_token"];

                await _cache.SetStringAsync(accessToken, "X",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2)
                    });

                // Remove token from cookie
                Response.Cookies.Delete("access_token");
            }

            if (Request.Cookies.ContainsKey("refresh_token"))
            {
                var refreshToken = Request.Cookies["refresh_token"];

                await _cache.SetStringAsync(refreshToken, "X",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2)
                    });

                // Remove token from cookie
                Response.Cookies.Delete("refresh_token");

                var refreshTokenKey = "Refresh:" + refreshToken;

                if (!string.IsNullOrEmpty(await _cache.GetStringAsync(refreshTokenKey)))
                {
                    await _cache.RemoveAsync(refreshTokenKey);
                }
            }

            return Ok();
        }
    }
}
