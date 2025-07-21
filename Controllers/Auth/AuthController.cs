using System.Security.Claims;
using JwtApi.Dtos;
using JwtApi.Models;
using JwtApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "아이디 또는 비밀번호가 잘못되었습니다." });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "입력값이 유효하지 않습니다." });

            try
            {
                await _userService.RegisterAsync(dto);
                return Ok(new { message = "회원가입 완료" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"서버 오류: {ex.Message}" });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("debug-role")]
        [AllowAnonymous]
        public IActionResult DebugRole()
        {
            return Ok(
                new
                {
                    RoleClaim = User.FindFirst(ClaimTypes.Role)?.Value, // ✅ ClaimTypes.Role 사용
                    IsAdmin = User.IsInRole("Admin"),
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                }
            );
        }
    }
}
