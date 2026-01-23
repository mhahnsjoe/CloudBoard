using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CloudBoard.Api.Middleware;

/// <summary>
/// Global exception handler implementing RFC 7807 ProblemDetails.
/// Maps domain exceptions to appropriate HTTP status codes.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = MapExceptionToProblem(exception, context);

        // Log with appropriate level
        LogException(exception, statusCode, context);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private (HttpStatusCode, ProblemDetails) MapExceptionToProblem(
        Exception exception,
        HttpContext context)
    {
        return exception switch
        {
            KeyNotFoundException ex => (
                HttpStatusCode.NotFound,
                CreateProblemDetails(
                    HttpStatusCode.NotFound,
                    "Resource Not Found",
                    ex.Message,
                    context)),

            InvalidOperationException ex => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Invalid Operation",
                    ex.Message,
                    context)),

            UnauthorizedAccessException ex => (
                HttpStatusCode.Forbidden,
                CreateProblemDetails(
                    HttpStatusCode.Forbidden,
                    "Access Denied",
                    ex.Message,
                    context)),

            ArgumentException ex => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    HttpStatusCode.BadRequest,
                    "Invalid Argument",
                    ex.Message,
                    context)),

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                CreateProblemDetails(
                    HttpStatusCode.Conflict,
                    "Concurrency Conflict",
                    "The resource was modified by another request. Please refresh and try again.",
                    context)),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(
                    HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred.",
                    context,
                    exception))
        };
    }

    private ProblemDetails CreateProblemDetails(
        HttpStatusCode statusCode,
        string title,
        string detail,
        HttpContext context,
        Exception? exception = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        // Include stack trace in development
        if (_environment.IsDevelopment() && exception != null)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problemDetails;
    }

    private void LogException(Exception exception, HttpStatusCode statusCode, HttpContext context)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Information,
            _ => LogLevel.Warning
        };

        _logger.Log(
            logLevel,
            exception,
            "HTTP {StatusCode} - {Method} {Path} - {Message}",
            (int)statusCode,
            context.Request.Method,
            context.Request.Path,
            exception.Message);
    }
}

/// <summary>
/// Extension method for clean middleware registration.
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
