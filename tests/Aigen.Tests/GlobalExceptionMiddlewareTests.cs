using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using CreateUserEnityInTheExistingDbContext.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace CreateUserEnityInTheExistingDbContext.Tests
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;
        private readonly GlobalExceptionMiddleware _middleware;

        public GlobalExceptionMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            _middleware = new GlobalExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn400_WhenValidationExceptionIsThrown()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new ValidationException("Validation failed"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            context.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn404_WhenNotFoundExceptionIsThrown()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new NotFoundException("Not found"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            context.Response.ContentType.Should().Be("application/problem+json");
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn500_WhenGeneralExceptionIsThrown()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new System.Exception("General error"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().Be("application/problem+json");
        }
    }
}