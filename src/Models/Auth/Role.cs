namespace ApiInABox.Models.Auth
{
    public class Role : AbstractDbBase
    {
        public string Name { get; set; }
        public User User { get; set; }
    }
}
