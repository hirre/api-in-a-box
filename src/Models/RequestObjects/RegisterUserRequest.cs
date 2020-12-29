using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models.RequestObjects
{
    public class RegisterUserRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ActivationEmail { get; set; }       
    }
}
