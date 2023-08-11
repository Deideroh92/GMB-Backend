using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace GMB.Place.Api.Core
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
            services.AddControllers();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Change this in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "vasano", // Replace with your JWT issuer
                    ValidAudience = "vasano-api", // Replace with your JWT audience
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key")) // Replace with your secret key
                };
            });

            services.AddAuthorization();

            services.AddControllers();

            services.AddHttpContextAccessor();
        }

        // Configure: Define how your application's request processing pipeline should be set up.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    Log.Information($"Matched endpoint: {endpoint.DisplayName}");
                } else
                {
                    Log.Information("No endpoint matched.");
                }
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection.
            //app.UseHttpsRedirection();

            // Serve static files (like HTML, CSS, JavaScript).
            app.UseStaticFiles();

            // Enable routing.
            app.UseRouting();

            // Authenticate users (if needed).
            app.UseAuthentication();

            // Authorize users based on policies (if needed).
            app.UseAuthorization();

            // Define how endpoints should be matched and handled.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization(); // Example
            });
        }
    }
}
