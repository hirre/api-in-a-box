
using System.Security;

namespace ApiInABox.Models.Auth
{
    public class Secret
    {
        public TokenData Token { get; } = new TokenData();

        public class TokenData
        {
            public SecureString Key { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }
    }
}
