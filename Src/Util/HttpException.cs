namespace Mercury.Util;

public class HttpException(string message, int statusCode = 500) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
