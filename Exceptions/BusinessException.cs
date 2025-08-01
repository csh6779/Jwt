namespace JwtApi.Exceptions
{
    public class BusinessException : Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message, int statuscode = 400)
            : base(message)
        {
            StatusCode = statuscode;
        }
    }
}
