/**
	Copyright 2021 Hirad Asadi (API in a Box)

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

		http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.
*/

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
        /// <summary>
        ///     Created the host.
        /// </summary>
        /// <param name="args">Host arguments</param>
        /// <returns>Host or exception</returns>
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
