using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApiService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = CreateHostBuilder(args).Build();
            await service.RunAsync();
            await service.StopAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
