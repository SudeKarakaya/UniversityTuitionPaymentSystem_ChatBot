using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace UniversityTuitionPaymentSystem.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthenticationController(IConfiguration config)
        {
            _config = config;
        }


        [HttpPost("token")]
        public IActionResult GetToken([FromBody] LoginRequest request)
        {

            if (request.Username == "bank" && request.Password == "bankpass")
            {
                var token = GenerateToken("Bank");
                return Ok(new { token });
            }

            if (request.Username == "admin" && request.Password == "adminpass")
            {
                var token = GenerateToken("Admin");
                return Ok(new { token });
            }

            return Unauthorized("Invalid username or password");
        }

        private string GenerateToken(string role)
        {
            var jwtKey = _config["Jwt:Key"] ?? "SUPER_SECRET_KEY_CHANGE_IN_PROD";
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Role, role)
        };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
