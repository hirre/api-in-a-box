using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ApiService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = ApiServiceFactory.Create(args);
            await service.RunAsync();
            await service.StopAsync();
        }
    }
}
