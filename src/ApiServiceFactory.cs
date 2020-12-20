using ApiService.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;

namespace ApiService
{
    public static class ApiServiceFactory
    {
        public static IHost Create(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
               .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               })
               .Build();

            return host;
        }
    }
}
