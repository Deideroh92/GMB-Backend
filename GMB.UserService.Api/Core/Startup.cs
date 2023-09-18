using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GMB.UserService.Api.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configure JWT authentication
            var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Secret"]);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "vasano",
                    ValidAudience = "vasano-api",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Use authentication
            app.UseAuthentication();
            app.UseCors("AllowAll");

            app.Use((context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                    context.Response.Headers.Add("Access-Control-Max-Age", "86400"); // 24 hours
                    context.Response.StatusCode = 204;
                    return context.Response.CompleteAsync();
                }
                return next();
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "user-service",
                    pattern: "user-service/api/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });
        }
    }
}
