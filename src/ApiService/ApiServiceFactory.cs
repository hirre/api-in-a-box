using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net;
using System.Reflection;
using Serilog;

namespace ApiInABox
{
    public static class ApiServiceFactory
    {
        public static IHost Create(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .AddJsonFile("appsettings.json")
                        .Build();

            var host = Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                })
               .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();

                   webBuilder.ConfigureKestrel(options =>
                   {
                       var httpsPort = int.Parse(configuration["https_port"]);

                       options.Listen(IPAddress.Any, httpsPort,
                                        listenOptions =>
                                        {
                                            listenOptions.UseHttps(configuration["https_certificate"],
                                                configuration["https_certificate_password"]);
                                        });
                   });
               })

               .Build();

            return host;
        }
    }
}
