using System;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ApiInABox.Contexts;
using ApiInABox.Exceptions;
using ApiInABox.Logic;
using ApiInABox.Middlewares;
using ApiInABox.Models.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace ApiInABox
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
            services.AddCors();

            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(new MailKitOptions()
                {
                    Server = Configuration["MailSetting:Host"],
                    Port = Convert.ToInt32(Configuration["MailSetting:Port"]),
                    SenderName = Configuration["MailSetting:SenderName"],
                    SenderEmail = Configuration["MailSetting:SenderEmail"],
                    Account = Configuration["MailSetting:Account"],
                    Password = Configuration["MailSetting:Password"],
                    // enable ssl or tls
                    Security = true
                });
            });

            services.AddDbContext<DatabaseContext>(options =>
                    options.UseNpgsql(Configuration["ConnectionString"], op =>
                    {
                        op.UseNodaTime();
                    }));

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
            services.AddScoped<RegisterLogic>();

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
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["Auth"];
                        return Task.CompletedTask;
                    },
                };
            });

            services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                        options.JsonSerializerOptions.WriteIndented = true;
                        options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
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

            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.AllowCredentials();
                policy.AllowAnyMethod();
            });

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
            app.UseMiddleware<IdentityMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
