using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ApiInABox
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

            var service = ApiServiceFactory.Create(args);
            await service.RunAsync();
            await service.StopAsync();
        }
    }
}
