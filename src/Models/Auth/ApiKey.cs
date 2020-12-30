using NodaTime;

namespace ApiInABox.Models.Auth
{
    public class ApiKey : AbstractDbBase
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public Instant ExpirationDate { get; set; }
    }
}
