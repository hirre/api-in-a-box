using ApiService.Models;

namespace Models
{
    public class Role : AbstractDbBase
    {
        public string Name { get; set; }
        public User User { get; set; }
    }
}
