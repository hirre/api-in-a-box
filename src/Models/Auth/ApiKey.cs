using System;
using ApiService.Models;

namespace Models.Auth
{
    public class ApiKey : AbstractDbBase
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
    }
}
