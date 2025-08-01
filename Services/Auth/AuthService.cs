using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using JwtApi.Data;
using JwtApi.Dtos;
using JwtApi.Models;
using Microsoft.EntityFrameworkCore;

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

            return new LoginResponseDto
            {
                Token = token,
                Username = user.Name,
                Role = user.Role,
            };
        }

        public async Task<ServiceResponseDto> LoginResponseAsync(LoginRequestDto dto)
        {
            var result = await LoginAsync(dto);
            if (result == null)
                return new ServiceResponseDto(
                    401,
                    new { message = "아이디 또는 비밀번호가 일치하지 않습니다." }
                );

            return new ServiceResponseDto(200, result);
        }
    }
}
