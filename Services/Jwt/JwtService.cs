using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtApi.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(int userId, string role)
        {
            var claims = new[]
            {
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Name, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            int expiresIn = 60;
            int.TryParse(_config["JwtSettings:ExpiresInMinutes"], out expiresIn);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresIn),
                signingCredentials: creds
            );

            Console.WriteLine("[🔑 JWT 발급용 SecretKey] " + _config["JwtSettings:SecretKey"]);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
