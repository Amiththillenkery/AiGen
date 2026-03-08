using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CreateADotnetRepositoryWithCleanArchitecture.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.TraceIdentifier;
            var statusCode = HttpStatusCode.InternalServerError;
            var problemDetails = new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "An unexpected error occurred!",
                Status = (int)statusCode,
                Detail = exception.Message,
                Instance = context.Request.Path,
                Extensions = { { "correlationId", correlationId } }
            };

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    problemDetails.Type = "https://httpstatuses.com/400";
                    problemDetails.Title = "Validation error";
                    problemDetails.Detail = validationException.Message;
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    problemDetails.Type = "https://httpstatuses.com/404";
                    problemDetails.Title = "Resource not found";
                    problemDetails.Detail = notFoundException.Message;
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    problemDetails.Type = "https://httpstatuses.com/401";
                    problemDetails.Title = "Unauthorized access";
                    problemDetails.Detail = unauthorizedAccessException.Message;
                    break;
                default:
                    _logger.LogError(exception, "An unhandled exception occurred. Correlation ID: {CorrelationId}", correlationId);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }
    }
}