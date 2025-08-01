using System.Text.Json;
using JwtApi.Exceptions;

namespace JwtApi.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // 다음 미들웨어로 전달
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "예외 발생!");

                context.Response.ContentType = "application/json";

                if (ex is BusinessException businessEx)
                {
                    context.Response.StatusCode = businessEx.StatusCode;
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(new { message = businessEx.Message })
                    );
                }
                else
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(new { message = "서버 내부 오류가 발생했습니다." })
                    );
                }
            }
        }
    }
}
