using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models.RequestObjects
{
    public class AuthUserRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
