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
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1),
                IsEssential = true
            };

            var refreshTokenOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(1),
                IsEssential = true
            };

            var refreshToken = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

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

                if (!string.IsNullOrEmpty(accessToken))
                {
                    var accessTokenOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(1),
                        IsEssential = true
                    };

                    var refreshTokenOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(1),
                        IsEssential = true
                    };

                    var newRefreshToken = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

                    // Remove old token from cache
                    await _cache.RemoveAsync(refreshTokenKey);

                    // Mark old token as disposed
                    await _cache.SetStringAsync(refreshToken, "X");

                    // Add new refresh token
                    await _cache.SetStringAsync("Refresh:" + newRefreshToken, accessToken);

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

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1),
                IsEssential = true
            };

            Response.Cookies.Append("access_token", token, options);
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
