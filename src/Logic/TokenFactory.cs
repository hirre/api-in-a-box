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