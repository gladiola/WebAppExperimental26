using WebAppExperimental26.Models.Storage;

namespace WebAppExperimental26.Tests.Models
{
    public class RedIdRecordCSVEntryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Act
            var entry = new RedIdRecordCSVEntry
            {
                FacilityCode = "FC123",
                CardNumber = "12345",
                CompanyName = "Test Facility",
                LocationAddress = "123 Main St",
                LocationCity = "Test City",
                LocationState = "TS",
                LocationZip = "12345",
                RFIDContent = "test-rfid"
            };

            // Assert
            entry.FacilityCode.Should().Be("FC123");
            entry.CardNumber.Should().Be("12345");
            entry.CompanyName.Should().Be("Test Facility");
            entry.LocationAddress.Should().Be("123 Main St");
            entry.LocationCity.Should().Be("Test City");
            entry.LocationState.Should().Be("TS");
            entry.LocationZip.Should().Be("12345");
        }

        [Fact]
        public void AllProperties_HaveDefaultValues()
        {
            // Act
            var entry = new RedIdRecordCSVEntry();

            // Assert
            entry.FacilityCode.Should().NotBeNull();
            entry.CardNumber.Should().NotBeNull();
            entry.CompanyName.Should().NotBeNull();
            entry.LocationAddress.Should().NotBeNull();
            entry.LocationCity.Should().NotBeNull();
            entry.LocationState.Should().NotBeNull();
            entry.LocationZip.Should().NotBeNull();
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        public void Properties_CanBeEmptyOrWhitespace(string value1, string value2, string value3)
        {
            // Act
            var entry = new RedIdRecordCSVEntry
            {
                FacilityCode = value1,
                CardNumber = value2,
                CompanyName = value3
            };

            // Assert
            entry.FacilityCode.Should().Be(value1);
            entry.CardNumber.Should().Be(value2);
            entry.CompanyName.Should().Be(value3);
        }

        [Fact]
        public void CSVEntry_CanHandleLongStrings()
        {
            // Arrange
            var longString = new string('A', 1000);

            // Act
            var entry = new RedIdRecordCSVEntry
            {
                CompanyName = longString,
                LocationAddress = longString
            };

            // Assert
            entry.CompanyName.Should().HaveLength(1000);
            entry.LocationAddress.Should().HaveLength(1000);
        }

        [Fact]
        public void CSVEntry_CanHandleSpecialCharacters()
        {
            // Act
            var entry = new RedIdRecordCSVEntry
            {
                CompanyName = "Test, Company & Associates",
                LocationAddress = "123 \"Main\" St.",
                LocationCity = "O'Brien"
            };

            // Assert
            entry.CompanyName.Should().Contain(",");
            entry.LocationAddress.Should().Contain("\"");
            entry.LocationCity.Should().Contain("'");
        }
    }
}
