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
                await RunNextAsync();
                return;
            }

            await BeforeAsync();
            if(Response.HasStarted) return;
            await RunAsync();
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

    public virtual void Before()
    {
    }

    public virtual Task BeforeAsync()
    {
        Before();
        return Task.CompletedTask;
    }

    public virtual async Task RunAsync()
    {
        await RunNextAsync();
    }

    public async Task RunNextAsync()
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
