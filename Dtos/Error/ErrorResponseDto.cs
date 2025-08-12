namespace JwtApi.Dtos;

public class ErrorResponseDto
{
    public required string Message { get; set; }
    public string? Detail { get; set; }
}
