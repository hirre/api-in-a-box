using System;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiService.Contexts;
using ApiService.Exceptions;
using ApiService.Logic;
using ApiService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ApiService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options =>
                    options.UseNpgsql(Configuration["ConnectionString"]));

            var secret = new Secret();
            var tokenSecKey = new SecureString();
            var tokenKeyBytes = Encoding.ASCII.GetBytes(Configuration["Token:Key"]);

            foreach (var c in Configuration["Token:Key"].ToCharArray())
            {
                tokenSecKey.AppendChar(c);
            }

            // Set key
            secret.Token.Key = tokenSecKey;

            services.AddSingleton(x => secret);
            services.AddSingleton<AuthLogic>();
            services.AddSingleton<RegisterLogic>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKeyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddControllers()
                    .AddJsonOptions(options => 
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                        options.JsonSerializerOptions.WriteIndented = true;
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DatabaseContext dbContext)
        {
#if DEBUG
            dbContext.Database.EnsureCreated();
#elif RELEASE
            dbContext.Database.Migrate();
#endif

            app.Use(next => async context => 
            {
                try
                {
                    await next.Invoke(context);
                }
                catch (Exception ex)
                {
                    var statusCode = 500;

                    if (ex is HttpException httpEx)
                        statusCode = httpEx.HttpStatusCode;

                    context.Response.StatusCode = statusCode;

                    var exMsg = ex.Message;
                    var ie = ex.InnerException;

                    while (ie != null)
                    {
                        exMsg += " " + ie.Message;
                        ie = ie.InnerException;
                    }
                    
                    var exObj = new 
                    { 
                        Exception = exMsg,   
                        HttpStatusCode = statusCode
                    };
                    
                    var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exObj));
                    context.Response.ContentType = "application/json";
                    await context.Response.Body.WriteAsync(data.AsMemory(0, data.Length));
                }
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); // Must occur before authorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
