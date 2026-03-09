using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImplementArticleEntitiy.Api.Middleware
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

            ProblemDetails problemDetails;
            int statusCode;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    problemDetails = new ValidationProblemDetails(new ModelStateDictionary())
                    {
                        Status = statusCode,
                        Title = "Validation Error",
                        Detail = validationException.Message,
                        Instance = context.Request.Path,
                        Extensions = { ["correlationId"] = correlationId }
                    };
                    break;

                case NotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "Resource Not Found",
                        Detail = exception.Message,
                        Instance = context.Request.Path,
                        Extensions = { ["correlationId"] = correlationId }
                    };
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "Unauthorized Access",
                        Detail = exception.Message,
                        Instance = context.Request.Path,
                        Extensions = { ["correlationId"] = correlationId }
                    };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    problemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = "An unexpected error occurred",
                        Detail = exception.Message,
                        Instance = context.Request.Path,
                        Extensions = { ["correlationId"] = correlationId }
                    };
                    break;
            }

            _logger.LogError(exception, "Exception caught in GlobalExceptionMiddleware. CorrelationId: {CorrelationId}", correlationId);

            context.Response.StatusCode = statusCode;
            var result = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(result);
        }
    }
}