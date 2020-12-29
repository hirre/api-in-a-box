using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models.RequestObjects
{
    public class RegisterApiKeyRequest
    {
        [Required]
        public string Name { get; set; }
    }
}
