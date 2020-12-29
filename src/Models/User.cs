using ApiInABox.Models.Auth;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models
{
    public class User : AbstractDbBase
    {
        public User()
        {
            Roles = new HashSet<Role>();
        }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ActivationEmail { get; set;}
        public bool Activated { get; set;}
        public string TemporarySecret { get; set; }
        public ICollection<Role> Roles { get; set; }
    }
}
