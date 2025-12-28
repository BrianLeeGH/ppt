using SlideBuilder.Api.Middleware;

namespace SlideBuilder.Api.Logging;

public static class LoggingExtensions
{
    public static IServiceCollection AddRequestLogging(this IServiceCollection services)
    {
        // For MVP, we'll just use the default console logger.
        // We can add Serilog or other providers here later.
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        return services;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ProblemDetailsMiddleware>>();
            logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await next();
            logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
        });

        return app;
    }
}
