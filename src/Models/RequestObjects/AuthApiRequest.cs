using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models.RequestObjects
{
    public class AuthApiRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Key { get; set; }
    }
}
