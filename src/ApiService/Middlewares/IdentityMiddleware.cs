using ApiInABox.Contexts;
using ApiInABox.Logic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ApiInABox.Middlewares
{
    public class IdentityMiddleware
    {
        private readonly RequestDelegate _next;

        public IdentityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DatabaseContext dbContext)
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.TryGetValue("Authorization", out var value);
                var bearerToken = value.ToString().Split(' ')[1];
                var nameId = TokenFactory.GetClaim(bearerToken, "nameid");
                var roles = TokenFactory.GetClaim(bearerToken, "role");

                if (roles != null)
                {
                    if (nameId != null && roles.Contains("user"))
                    {
                        dbContext.TokenNameId = nameId;
                    }
                }                
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
