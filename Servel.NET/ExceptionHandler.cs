using Microsoft.AspNetCore.Diagnostics;

namespace Servel.NET;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if(exception is FileNotFoundException || exception is DirectoryNotFoundException)
        {
            await Results.NotFound().ExecuteAsync(httpContext);
            return true;
        }

        return false;
    }
}
