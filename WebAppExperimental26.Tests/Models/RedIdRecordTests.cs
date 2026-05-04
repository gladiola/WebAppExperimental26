using WebAppExperimental26.Models.Storage;

namespace WebAppExperimental26.Tests.Models
{
    public class RedIdRecordTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Act
            var record = new RedIdRecord
            {
                Id = 1,
                FacilityCode = "FC123",
                CardNumber = "12345",
                CompanyName = "Test Facility",
                LocationAddress = "123 Test St",
                LocationCity = "Test City",
                LocationState = "TS",
                LocationZip = "12345",
                RFIDContent = "test-rfid"
            };

            // Assert
            record.Id.Should().Be(1);
            record.FacilityCode.Should().Be("FC123");
            record.CardNumber.Should().Be("12345");
            record.CompanyName.Should().Be("Test Facility");
        }

        [Fact]
        public void AllProperties_CanBeNull()
        {
            // Act
            var record = new RedIdRecord();

            // Assert
            record.Id.Should().Be(0);
            record.FacilityCode.Should().NotBeNull(); // Has default value
            record.CardNumber.Should().NotBeNull(); // Has default value
            record.CompanyName.Should().NotBeNull(); // Has default value
        }

        [Fact]
        public void Id_CanBeSetToZero()
        {
            // Act
            var record = new RedIdRecord { Id = 0 };

            // Assert
            record.Id.Should().Be(0);
        }

        [Fact]
        public void Id_CanBeSetToNegative()
        {
            // Act
            var record = new RedIdRecord { Id = -1 };

            // Assert
            record.Id.Should().Be(-1);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void StringProperties_CanBeEmptyOrNull(string? value)
        {
            // Act
            var record = new RedIdRecord
            {
                FacilityCode = value,
                CardNumber = value,
                CompanyName = value
            };

            // Assert
            record.FacilityCode.Should().Be(value);
            record.CardNumber.Should().Be(value);
            record.CompanyName.Should().Be(value);
        }
    }
}
