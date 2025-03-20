namespace Servel.NET;

public class MiddlewareBase(RequestDelegate next)
{
    private readonly AsyncLocal<HttpContext?> _httpContextHolder = new();
    public HttpContext HttpContext => _httpContextHolder.Value!;
    public HttpRequest Request => HttpContext.Request;
    public HttpResponse Response => HttpContext.Response;
    public ConnectionInfo Connection => HttpContext.Connection;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            _httpContextHolder.Value = httpContext;
            if(!await ShouldRunAsync())
            {
                await InvokeNext();
                return;
            }

            var result = await BeforeAsync();
            if(result != null)
            {
                await result.ExecuteAsync(HttpContext);
                return;
            }

            result = await RunAsync();
            if(result != null) await result.ExecuteAsync(HttpContext);
            else await InvokeNext();

            await AfterAsync();
        }
        finally
        {
            _httpContextHolder.Value = null;
        }
    }

    public virtual bool ShouldRun()
    {
        return true;
    }

    public virtual Task<bool> ShouldRunAsync()
    {
        return Task.FromResult(ShouldRun());
    }

    public virtual IResult? Before()
    {
        return null;
    }

    public virtual Task<IResult?> BeforeAsync()
    {
        return Task.FromResult(Before());
    }

    public virtual IResult? Run()
    {
        return null;
    }

    public virtual Task<IResult?> RunAsync()
    {
        return Task.FromResult(Run());
    }

    public async Task InvokeNext()
    {
        await next.Invoke(HttpContext);
    }

    public virtual void After()
    {
    }

    public virtual Task AfterAsync()
    {
        After();
        return Task.CompletedTask;
    }
}
