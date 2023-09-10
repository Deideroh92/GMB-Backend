using GMB.Sdk.Core.Types.Api;
using GMB.Sdk.Core.Types.Database.Manager;
using GMB.Sdk.Core.Types.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var test = _configuration["Jwt:Secret"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddHours(10), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "vasano",
                Audience = "vasano-api"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
