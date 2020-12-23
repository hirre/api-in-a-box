using Models;
using System.Collections.Generic;

namespace ApiService.Models
{
    public class User : AbstractDbBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public ICollection<Role> Roles { get; set; }
    }
}
