using System.Security;
using System.Text;
using ApiService.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
                    ValidIssuer = Configuration["Token:Issuer"],
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKeyBytes),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
                
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Generic API Service", Version = "v1" });
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                { 
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Generic API Service v1");
                    c.RoutePrefix = "api";
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class Secret
    {
        public TokenData Token { get; } = new TokenData();

        public class TokenData
        {
            public SecureString Key { get; set; }
            public string Issuer { get; set; }
        } 
    }
}
