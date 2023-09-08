namespace Servel.NET
{
    public class HomeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<Listing> _listings;

        public HomeMiddleware(RequestDelegate next, IEnumerable<Listing> listings)
        {
            _next = next;
            _listings = listings;

        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (FileHelpers.IsGetOrHeadMethod(httpContext.Request.Method)
                && httpContext.Request.Path == "/")
            {
                await Results.Extensions.View("Home", new { Listings = _listings }).ExecuteAsync(httpContext);
                return;
            }

            await _next.Invoke(httpContext);
        }
    }
}
