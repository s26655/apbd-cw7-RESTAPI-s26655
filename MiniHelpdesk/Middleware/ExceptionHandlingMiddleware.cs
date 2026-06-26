namespace MiniHelpdesk.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/html; charset=utf-8";

            await context.Response.WriteAsync("""
                <!doctype html>
                <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <title>Application error</title>
                    <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
                </head>
                <body>
                    <main class="container mt-5">
                        <div class="alert alert-danger">
                            <h1 class="h4">Something went wrong</h1>
                            <p>The application could not complete the request. Please try again later.</p>
                            <a href="/Tickets" class="btn btn-primary">Back to tickets</a>
                        </div>
                    </main>
                </body>
                </html>
                """);
        }
    }
}