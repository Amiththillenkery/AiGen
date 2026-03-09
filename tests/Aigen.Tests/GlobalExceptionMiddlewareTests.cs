using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImplementArticleEntitiy.Tests
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;
        private readonly RequestDelegate _next;
        private readonly GlobalExceptionMiddleware _middleware;

        public GlobalExceptionMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            _next = (HttpContext context) => Task.CompletedTask;
            _middleware = new GlobalExceptionMiddleware(_next, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn400_WhenValidationExceptionThrown()
        {
            var context = new DefaultHttpContext();
            _next = (HttpContext ctx) => throw new ValidationException("Validation failed");

            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            _loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn404_WhenNotFoundExceptionThrown()
        {
            var context = new DefaultHttpContext();
            _next = (HttpContext ctx) => throw new NotFoundException("Resource not found");

            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            _loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn500_WhenGeneralExceptionThrown()
        {
            var context = new DefaultHttpContext();
            _next = (HttpContext ctx) => throw new Exception("General error");

            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _loggerMock.Verify(log => log.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}