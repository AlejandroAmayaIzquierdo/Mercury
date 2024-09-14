using Mercury.Exeptions;
using Newtonsoft.Json;

namespace Mercury;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var excludeFromWrapper = endpoint?.Metadata.GetMetadata<ExcludeResponseWrapper>() != null;

        if (excludeFromWrapper)
        {
            await _next(context);
            return;
        }


        var originalBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);

            if (IsErrorCode(context.Response.StatusCode))
            {
                context.Response.Body = originalBodyStream;
                var errorResponse = new { error = 1, Detail = GetErrorMessage(context.Response.StatusCode) };
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
                return;
            }

            context.Response.Body = originalBodyStream;
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            var wrappedResponse = new { error = 0, Detail = JsonConvert.DeserializeObject(responseBody) };

            //context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(wrappedResponse));
        }
        catch (Exception ex)
        {
            int statusCode = 500; // default to 500
            string errorMessage = ex.Message;

            if (ex is HttpException httpException)
            {
                statusCode = httpException.StatusCode;
                errorMessage = httpException.Message;
            }


            context.Response.StatusCode = statusCode;
            context.Response.Body = originalBodyStream;
            var errorResponse = new { error = 1, Detail = errorMessage };
            context.Response.ContentType = "application/json";
            WebService.GetLogger()?.Error(JsonConvert.SerializeObject(errorResponse));
            await context.Response.WriteAsync(JsonConvert.SerializeObject(errorResponse));
        }
    }

    private static bool IsErrorCode(int code)
    {
        return code >= 400 && code <= 599;
    }
    private static string GetErrorMessage(int code)
    {
        return code switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            402 => "Payment Required",
            403 => "Forbidden",
            404 => "Route not found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            407 => "Proxy Authentication Required",
            408 => "Request Timeout",
            409 => "Conflict",
            410 => "Gone",
            411 => "Length Required",
            412 => "Precondition Failed",
            413 => "Payload Too Large",
            414 => "URI Too Long",
            415 => "Unsupported Media Type",
            416 => "Range Not Satisfiable",
            417 => "Expectation Failed",
            418 => "I'm a teapot",
            421 => "Misdirected Request",
            422 => "Unprocessable Entity",
            423 => "Locked",
            424 => "Failed Dependency",
            426 => "Upgrade Required",
            428 => "Precondition Required",
            429 => "Too Many Requests",
            431 => "Request Header Fields Too Large",
            451 => "Unavailable For Legal Reasons",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            505 => "HTTP Version Not Supported",
            506 => "Variant Also Negotiates",
            507 => "Insufficient Storage",
            508 => "Loop Detected",
            510 => "Not Extended",
            511 => "Network Authentication Required",
            _ => "Unknown Error",
        };
    }

}

[AttributeUsage(AttributeTargets.Method)]
public class ExcludeResponseWrapper : Attribute { }
