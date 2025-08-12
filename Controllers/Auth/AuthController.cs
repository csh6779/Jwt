using System.Security.Claims;
using JwtApi.Dtos;
using JwtApi.Models;
using JwtApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(
            Summary = "로그인",
            Description = "아이디와 비밀번호를 이용하여 로그인합니다.",
            Tags = new[] { "로그인 및 회원가입" }
        )]
        [SwaggerResponse(200, "로그인 성공", typeof(LoginResponseDto))] // 반환 데이터 타입 명시
        [SwaggerResponse(400, "입력값 오류")]
        [SwaggerResponse(401, "아이디 또는 비밀번호가 잘못되었습니다.")]
        [SwaggerResponse(500, "서버 오류")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var response = await _authService.LoginResponseAsync(dto);
            return StatusCode(response.StatusCode, response.Body);
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "회원가입",
            Description = "회원가입합니다.",
            Tags = new[] { "로그인 및 회원가입" }
        )]
        [SwaggerResponse(200, "회원가입 성공")]
        [SwaggerResponse(409, "중복된 사용자 ID")]
        [SwaggerResponse(400, "입력값 오류")]
        [SwaggerResponse(500, "서버 오류")]
        public async Task<IActionResult> Register([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "입력값이 유효하지 않습니다." });

            var response = await _userService.RegisterResponseAsync(dto);
            return StatusCode(response.StatusCode, response.Body);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "회원 리스트",
            Description = "회원가입된 회원",
            Tags = new[] { "Debugging" }
        )]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("debug-role")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Role 리스트",
            Description = "회원가입된 회원의 Role",
            Tags = new[] { "Debugging" }
        )]
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

        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Refresh Token",
            Description = "Refresh Token을 이용해 새로운 Access Token을 발급받습니다.",
            Tags = new[] { "로그인 및 회원가입" }
        )]
        [SwaggerResponse(200, "토큰 재발급 성공", typeof(LoginResponseDto))]
        [SwaggerResponse(401, "Refresh Token이 유효하지 않음")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequestDto dto)
        {
            var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return StatusCode(response.StatusCode, response.Body);
        }

        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Refresh Token삭제",
            Description = "로그아웃시 refreshtoken을 삭제합니다.",
            Tags = new[] { "로그인 및 회원가입" }
        )]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var response = await _authService.LogoutAsync(userId);
            return StatusCode(response.StatusCode, response.Body);
        }
    }
}
