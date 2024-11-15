using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration; // Inject IConfiguration for reading JWT settings
        private readonly AuthAPIContext _context;

        public AuthController(IConfiguration configuration, AuthAPIContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO loginDto)
        {

        // find whether a user exist with the given email
        var user = _context.User.FirstOrDefault(u => u.Email == loginDto.Email);
        Console.WriteLine(user.Email);

        if (!(user != null && VerifyPassword(user.Password, loginDto.Password))) return Unauthorized();
            
            var token = GenerateJwtToken(user.Email);
            return Ok(new {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.MobileNumber,
                user.Email,
                Image = "profile.png",
                JwtToken = token });
        }

        // Generate JWT token
        private string GenerateJwtToken(string email)
        {
        //var jwtSettings = _configuration.GetSection("JwtSettings");
        //var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
        //var issuer = jwtSettings["Issuer"];
        //var audience = jwtSettings["Audience"];
        //var tokenExpiration = TimeSpan.FromMinutes(double.Parse(jwtSettings["DurationInMinutes"]));
        var key =Encoding.ASCII.GetBytes( Environment.GetEnvironmentVariable("JWT_KEY"));
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var tokenExpiration =TimeSpan.FromMinutes(double.Parse( Environment.GetEnvironmentVariable("JWT_DURATION_IN_MINUTES")));

        var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(tokenExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        private static bool VerifyPassword(string storedPasswordHash, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, storedPasswordHash);
        }
    }