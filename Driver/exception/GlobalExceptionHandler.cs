using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    
    private ILogger<GlobalExceptionHandler> Logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        Logger=logger;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int StatusCode;
        string message;
        if(exception is CustomException customException)
        {
            Logger.LogWarning("Request {Path} failed with {StatusCode}: {Message}",
            httpContext.Request.Path, customException.StatusCode, customException.Message);
            StatusCode=customException.StatusCode;
            message=customException.Message;
        }
        else
        {
            Logger.LogError(exception,"unhandled error");
            StatusCode=500;
            message="internal server error";
        }
        var response = new ApiResponse<object>(false,message,null);

        httpContext.Response.StatusCode=StatusCode;
        await httpContext.Response.WriteAsJsonAsync(response,cancellationToken);
        return true;
    }
}