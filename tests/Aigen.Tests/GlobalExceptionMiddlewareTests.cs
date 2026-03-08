using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CreateADotnetRepositoryWithCleanArchitecture.Middlewares;
using CreateADotnetRepositoryWithCleanArchitecture.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CreateADotnetRepositoryWithCleanArchitecture.Tests
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
        public async Task InvokeAsync_ValidationException_ReturnsBadRequest()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new ValidationException("Validation failed"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_NotFoundException_ReturnsNotFound()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new NotFoundException("Not found"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_GeneralException_ReturnsInternalServerError()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new System.Exception("General error"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ValidationException_ReturnsProblemDetails()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new ValidationException("Validation failed"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = new System.IO.StreamReader(context.Response.Body).ReadToEnd();
            var problemDetails = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            Assert.Equal("Validation failed", problemDetails.Title);
            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
        }

        [Fact]
        public async Task InvokeAsync_NotFoundException_ReturnsProblemDetails()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new NotFoundException("Not found"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = new System.IO.StreamReader(context.Response.Body).ReadToEnd();
            var problemDetails = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            Assert.Equal("Not found", problemDetails.Title);
            Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
        }

        [Fact]
        public async Task InvokeAsync_GeneralException_ReturnsProblemDetails()
        {
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new System.Exception("General error"));

            var context = new DefaultHttpContext();
            await _middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var responseBody = new System.IO.StreamReader(context.Response.Body).ReadToEnd();
            var problemDetails = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            Assert.Equal("An unexpected error occurred.", problemDetails.Title);
            Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
        }
    }
}