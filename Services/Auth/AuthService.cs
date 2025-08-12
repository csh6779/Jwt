using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using JwtApi.Data;
using JwtApi.Dtos;
using JwtApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JwtApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, IConfiguration config, JwtService jwtService)
        {
            _context = context;
            _config = config;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return null; // 로그인 실패
            }

            var token = _jwtService.GenerateToken(user.Id, user.Role);
            var refreshtoken = _jwtService.GenerateRefreshToken(user.Id);

            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshtoken,
                Username = user.Name,
                Role = user.Role,
            };
        }

        public async Task<ServiceResponseDto<object>> LoginResponseAsync(LoginRequestDto dto)
        {
            var result = await LoginAsync(dto);
            if (result == null)
                return new ServiceResponseDto<object>(
                    401,
                    new { message = "아이디 또는 비밀번호가 일치하지 않습니다." }
                );

            return new ServiceResponseDto<object>(200, result);
        }

        public async Task<ServiceResponseDto<object>> RefreshTokenAsync(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    refreshToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["JwtSettings:Issuer"],
                        ValidAudience = _config["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!)
                        ),
                        ClockSkew = TimeSpan.Zero,
                    },
                    out var validatedToken
                );

                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                if (tokenType != "Refresh")
                    return new ServiceResponseDto<object>(
                        101,
                        new { message = "Refresh Token이 아닙니다." }
                    );

                var userId = int.Parse(principal.Claims.First(c => c.Type == "UserId").Value);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (
                    user == null
                    || user.RefreshToken != refreshToken
                    || user.RefreshTokenExpiryTime < DateTime.UtcNow
                )
                {
                    return new ServiceResponseDto<object>(
                        401,
                        new { message = "Refresh Token이 유효하지 않습니다." }
                    );
                }

                // 새로운 토큰 발급
                var newAccessToken = _jwtService.GenerateToken(user.Id, user.Role);
                var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                var response = new LoginResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Username = user.Name,
                    Role = user.Role,
                };

                return new ServiceResponseDto<object>(200, response);
            }
            catch
            {
                return new ServiceResponseDto<object>(
                    401,
                    new { message = "Refresh Token이 유효하지 않거나 만료되었습니다." }
                );
            }
        }

        public async Task<ServiceResponseDto<object>> LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponseDto<object>(
                    404,
                    new { message = "사용자를 찾을 수 없습니다." }
                );
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();

            return new ServiceResponseDto<object>(200, new { message = "로그아웃되었습니다." });
        }
    }
}
