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
            StatusCode=customException.StatusCode;
            message=customException.Message;
        }
        else
        {
            Logger.LogError(exception,"unhandled error");
            StatusCode=500;
            message="internal server error";
        }
        var log = new ProblemDetails();
        log.Status=StatusCode;
        log.Title=message;
        
        httpContext.Response.StatusCode=StatusCode;
        await httpContext.Response.WriteAsJsonAsync(log,cancellationToken);
        return true;
    }
}