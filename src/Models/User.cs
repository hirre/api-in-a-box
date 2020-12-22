namespace ApiService.Models
{
    public class User : AbstractDbBase
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
