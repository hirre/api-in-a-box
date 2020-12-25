using ApiService.Models;

namespace ApiService.Models.Auth
{
    public class Role : AbstractDbBase
    {
        public string Name { get; set; }
        public User User { get; set; }
    }
}
