namespace JwtApi.Dtos;

public class ServiceResponseDto<T>
{
    public int StatusCode { get; set; }
    public T Body { get; set; }

    public ServiceResponseDto(int statuscode, T body)
    {
        StatusCode = statuscode;
        Body = body;
    }
}
