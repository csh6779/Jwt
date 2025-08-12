using BCrypt.Net;
using JwtApi.Data;
using JwtApi.Dtos;
using JwtApi.Exceptions;
using JwtApi.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtApi.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task RegisterAsync(UserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserId == dto.UserId))
            {
                throw new BusinessException("이미 존재하는 사용자입니다.", 409);
            }

            var hashedPassword = dto.Password;

            var user = new User
            {
                UserId = dto.UserId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = "User",
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<ServiceResponseDto<object>> RegisterResponseAsync(UserDto dto)
        {
            try
            {
                await RegisterAsync(dto);
                return new ServiceResponseDto<object>(200, new { message = "회원가입 완료!" });
            }
            catch (InvalidOperationException ex)
            {
                return new ServiceResponseDto<object>(400, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ServiceResponseDto<object>(
                    500,
                    new { message = $"서버 오류 : {ex.Message}" }
                );
            }
        }
    }
}
