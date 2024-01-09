using GMB.ScannerService.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();
var isDevelopmentEnvironment = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins", policy =>
    {
        if (isDevelopment)
        {
            policy.AllowAnyOrigin();
        } else
        {
            policy.WithOrigins("https://vasanogmbapi.azurewebsites.net", "https://admin-vasano.web.app", "http://localhost:3000");
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<AuthorizationPolicyService>();

if (isDevelopment)
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.RequireAssertion(context => true); // Allow unauthenticated access during development
        });
}

if (!isDevelopmentEnvironment)
{
    builder.Services.AddAuthentication(options =>
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("fjQYTZrDDp8hlPFQD3IxibbNsSF6bLTC4TI98XUJ3e4nZhGFmMgP4hsGbiaoNydG"))
        };
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
    });

    builder.Services.AddAuthorization();
}

var app = builder.Build();

app.UseHttpsRedirection();

// Use CORS before authorization
app.UseCors("_myAllowSpecificOrigins");

if (!isDevelopmentEnvironment)
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
