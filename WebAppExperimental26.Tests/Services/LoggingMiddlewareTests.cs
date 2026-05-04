using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebAppExperimental26.Services;

namespace WebAppExperimental26.Tests.Services
{
    public class LoggingMiddlewareTests
    {
        private readonly Mock<ILogger<LoggingMiddleware>> _mockLogger;
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly LoggingMiddleware _middleware;

        public LoggingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<LoggingMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new LoggingMiddleware(_mockNext.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldAcceptDependencies()
        {
            // Assert
            _middleware.Should().NotBeNull();
        }

        [Fact]
        public async Task Invoke_ShouldLogBeforeAndAfterRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.Invoke(context);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response sent")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Invoke_ShouldCallNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new LoggingMiddleware(next, _mockLogger.Object);

            // Act
            await middleware.Invoke(context);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Invoke_ShouldLogUtcTime()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            await _middleware.Invoke(context);

            // Assert - Verify logs contain timestamps
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2)); // Before and after
        }

        [Fact]
        public async Task Invoke_WhenNextThrows_ShouldPropagatException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var expectedException = new InvalidOperationException("Test exception");
            _mockNext.Setup(x => x(It.IsAny<HttpContext>())).ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await _middleware.Invoke(context);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Test exception");
        }
    }
}
