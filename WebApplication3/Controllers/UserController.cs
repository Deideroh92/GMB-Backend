using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GMB.UserService.Api.Controller
{
    [Route("api/user-service")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Login into app with username and password
        /// </summary>
        /// <param name="request"></param>
        /// <returns>JWT Token if success, or NotFound if fail</returns>
        [HttpPost("login")]
        public ActionResult<GetLoginResponse> Login(GetLoginRequest request)
        {
            if (request.Login == null || request.Password == null)
                return GetLoginResponse.Exception("Authentication failed. Username or password is/are null.");

            using DbLib db = new();
            DbUser? user = db.GetUser(request.Login, request.Password);

            if (user == null)
                return GetLoginResponse.Exception("Authentication failed. Username or password incorrect.");

            string token = GenerateJwtToken(user.Login);

            return new GetLoginResponse(user.Login, token);
        }

        /// <summary>
        /// Generate JWT Token for authentication
        /// </summary>
        /// <param name="username"></param>
        /// <returns>JWT Token</returns>
        private string GenerateJwtToken(string username)
        {
            
            var key = "fjQYTZrDDp8hlPFQD3IxibbNsSF6bLTC4TI98XUJ3e4nZhGFmMgP4hsGbiaoNydG";


            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Aud, "vasano-api"),
                new Claim(JwtRegisteredClaimNames.Iss, "vasano")
                // Add more claims as needed
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), // Token expiration time
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
