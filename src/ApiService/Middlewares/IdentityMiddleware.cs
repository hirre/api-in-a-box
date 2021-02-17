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
                        // Set the user name id property in the database context based on token information
                        dbContext.TokenNameId = nameId;
                    }
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
