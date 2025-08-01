namespace JwtApi.Dtos;

public class ServiceResponseDto
{
    public int StatusCode { get; set; }
    public object Body { get; set; }

    public ServiceResponseDto(int statuscode, object body)
    {
        StatusCode = statuscode;
        Body = body;
    }
}
