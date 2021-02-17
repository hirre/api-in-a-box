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
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace ApiInABox.Logic
{
    public static class TokenFactory
    {
        /// <summary>
        ///     Creats a new access token.
        /// </summary>
        /// <param name="secret">Token secret</param>
        /// <param name="issuer">Issuer</param>
        /// <param name="audience">Audience</param>
        /// <param name="expirationDate">Expiration date</param>
        /// <param name="claims">List of claims</param>
        /// <returns>Token or exception</returns>
        public static string Generate(SecureString secret, string issuer, string audience,
            DateTime expirationDate, Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationDate,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecureStringToString(secret))),
                     SecurityAlgorithms.HmacSha256Signature)
            }));
        }

        /// <summary>
        ///     Validates a token.
        /// </summary>
        /// <param name="token">The token to be validated</param>
        /// <param name="secret">Token secret</param>
        /// <param name="issuer">Issuer</param>
        /// <param name="audience">Audience</param>
        /// <param name="tvp">Token validation parameters</param>
        /// <returns>Tuple containing the validation result (true success, false failure) along with the claims and exception</returns>
        public static (bool ValidationResult, ClaimsPrincipal ClaimsPrincipal, Exception Exception) Validate(string token, SecureString secret,
            string issuer, string audience, TokenValidationParameters tvp = null)
        {
            ClaimsPrincipal claimsPrincipal = null;

            try
            {
                if (tvp == null)
                    tvp = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        LifetimeValidator = TokenLifetimeValidator.Validate,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecureStringToString(secret)))
                    };

                claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, tvp, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                return (false, claimsPrincipal, ex);
            }

            return (true, claimsPrincipal, null);
        }

        /// <summary>
        ///     Get claims from a token.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="claimType">Claim type</param>
        /// <returns>Claim value.</returns>
        public static string GetClaim(string token, string claimType)
        {
            try
            {

                var claimValue = (new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken)
                        .Claims.First(claim => claim.Type == claimType).Value;

                return claimValue;
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }

    public static class TokenLifetimeValidator
    {
        public static bool Validate(DateTime? notBefore, DateTime? expires,
            SecurityToken tokenToValidate, TokenValidationParameters @param)
        {
            return (expires != null && expires > DateTime.UtcNow);
        }
    }
}