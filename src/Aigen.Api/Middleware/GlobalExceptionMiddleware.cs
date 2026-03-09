using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FluentValidation;
using CreateUserEnityInTheExistingDbContext.Api.Exceptions;

namespace CreateUserEnityInTheExistingDbContext.Api.Middleware
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
            context.Response.ContentType = "application/json";
            var correlationId = context.TraceIdentifier;
            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Title = "An error occurred while processing your request.",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = exception.Message,
                Extensions = { { "correlationId", correlationId } }
            };

            switch (exception)
            {
                case ValidationException validationException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Validation error";
                    problemDetails.Detail = validationException.Message;
                    break;
                case NotFoundException:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "Resource not found";
                    break;
                case UnauthorizedAccessException:
                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Title = "Unauthorized access";
                    break;
                default:
                    _logger.LogError(exception, "Unhandled exception occurred. Correlation ID: {CorrelationId}", correlationId);
                    break;
            }

            context.Response.StatusCode = problemDetails.Status.Value;
            var jsonResponse = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}