using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GMB.BusinessService.Api.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ConfigureServices: Add and configure services needed by your application.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers to handle API requests.
            services.AddControllers().AddNewtonsoftJson();

            // Configure JWT authentication
            var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Secret"]) ?? throw new InvalidOperationException("JWT secret key is missing or invalid.");

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

        // Configure: Define how your application's request processing pipeline should be set up.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // Use authentication
            app.UseAuthentication();

            // Authorize users based on policies (if needed).
            app.UseAuthorization();

            

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

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

            // Define how endpoints should be matched and handled.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "business-service",
                    pattern: "business-service/api/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers(); // Example
            });
        }
    }
}
