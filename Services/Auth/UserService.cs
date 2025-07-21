using BCrypt.Net;
using JwtApi.Data;
using JwtApi.Dtos;
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
            // 1. 중복 검사
            if (await _context.Users.AnyAsync(u => u.UserId == dto.UserId))
            {
                throw new InvalidOperationException("이미 존재하는 사용자 ID입니다.");
            }

            // 2. 비밀번호 해싱 (예시 - 실제 적용 시 보안 강화 필요)
            var hashedPassword = dto.Password; // TODO: 실제로는 해싱 처리

            // 3. User 모델 매핑 및 저장
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
    }
}
