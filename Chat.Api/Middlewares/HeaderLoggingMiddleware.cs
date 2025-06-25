public class HeaderLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HeaderLoggingMiddleware> _logger;

    public HeaderLoggingMiddleware(RequestDelegate next, ILogger<HeaderLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if ((method == HttpMethods.Post || method == HttpMethods.Put || method == HttpMethods.Delete)
            && !path.Contains("service.svc"))
        {
            var source = context.Request.Headers["X-Request-Source"].FirstOrDefault();
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(userAgent))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing required headers: X-Request-Source and/or User-Agent");
                return;
            }
        }

        var logSource = context.Request.Headers["X-Request-Source"].FirstOrDefault() ?? "Brak";
        var logUserAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Brak";

        _logger.LogInformation("Źródło żądania: {Source}, User-Agent: {UserAgent}", logSource, logUserAgent);

        await _next(context);
    }
}
