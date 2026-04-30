using WebAppExperimental26.Models.Main_Objects;

namespace WebAppExperimental26.Tests.Models
{
    public class ErrorResponseTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties()
        {
            // Arrange
            var errorMessage = "Test error message";
            var statusCode = 400;

            // Act
            var errorResponse = new ErrorResponse
            {
                Message = errorMessage,
                StatusCode = statusCode
            };

            // Assert
            errorResponse.Message.Should().Be(errorMessage);
            errorResponse.StatusCode.Should().Be(statusCode);
        }

        [Fact]
        public void DefaultConstructor_ShouldCreateEmptyObject()
        {
            // Act
            var errorResponse = new ErrorResponse();

            // Assert
            errorResponse.Message.Should().BeNull();
            errorResponse.StatusCode.Should().Be(0);
        }

        [Theory]
        [InlineData(400, "Bad Request")]
        [InlineData(401, "Unauthorized")]
        [InlineData(403, "Forbidden")]
        [InlineData(404, "Not Found")]
        [InlineData(500, "Internal Server Error")]
        public void StatusCode_ShouldAcceptCommonHttpCodes(int statusCode, string message)
        {
            // Act
            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            };

            // Assert
            errorResponse.StatusCode.Should().Be(statusCode);
            errorResponse.Message.Should().Be(message);
        }

        [Fact]
        public void Message_CanBeEmpty()
        {
            // Act
            var errorResponse = new ErrorResponse
            {
                Message = string.Empty,
                StatusCode = 400
            };

            // Assert
            errorResponse.Message.Should().BeEmpty();
        }
    }
}
