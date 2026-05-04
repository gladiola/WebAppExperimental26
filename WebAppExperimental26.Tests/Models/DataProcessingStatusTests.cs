using WebAppExperimental26.Models.Main_Objects;

namespace WebAppExperimental26.Tests.Models
{
    public class DataProcessingStatusTests
    {
        [Fact]
        public void AllStatuses_ShouldHaveUniqueIds()
        {
            // Arrange
            var allStatuses = Enumeration.GetAll<DataProcessingStatus>();

            // Act
            var uniqueIds = allStatuses.Select(s => s.Id).Distinct();

            // Assert
            uniqueIds.Should().HaveCount(allStatuses.Count());
        }

        [Fact]
        public void AllStatuses_ShouldHaveUniqueNames()
        {
            // Arrange
            var allStatuses = Enumeration.GetAll<DataProcessingStatus>();

            // Act
            var uniqueNames = allStatuses.Select(s => s.Name).Distinct();

            // Assert
            uniqueNames.Should().HaveCount(allStatuses.Count());
        }

        [Fact]
        public void Success_ShouldHaveCorrectValues()
        {
            // Assert
            DataProcessingStatus.Success.Name.Should().Be("Success");
            DataProcessingStatus.Success.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Failure_ShouldHaveCorrectValues()
        {
            // Assert
            DataProcessingStatus.Failure.Name.Should().Be("Failure");
            DataProcessingStatus.Failure.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Info_ShouldHaveCorrectValues()
        {
            // Assert
            DataProcessingStatus.Info.Name.Should().Be("Info");
            DataProcessingStatus.Info.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Exception_ShouldHaveCorrectValues()
        {
            // Assert
            DataProcessingStatus.Exception.Name.Should().Be("Exception");
            DataProcessingStatus.Exception.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ToString_ShouldReturnName()
        {
            // Act & Assert
            DataProcessingStatus.Success.ToString().Should().Be("Success");
            DataProcessingStatus.Failure.ToString().Should().Be("Failure");
        }

        [Fact]
        public void Equals_SameStatus_ShouldReturnTrue()
        {
            // Act
            var result = DataProcessingStatus.Success.Equals(DataProcessingStatus.Success);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentStatus_ShouldReturnFalse()
        {
            // Act
            var result = DataProcessingStatus.Success.Equals(DataProcessingStatus.Failure);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("not a status")]
        public void Equals_InvalidObject_ShouldReturnFalse(object? obj)
        {
            // Act
            var result = DataProcessingStatus.Success.Equals(obj);

            // Assert
            result.Should().BeFalse();
        }
    }
}
