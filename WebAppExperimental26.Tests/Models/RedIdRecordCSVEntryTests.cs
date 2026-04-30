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
                Name = "Test Facility",
                Address = "123 Main St",
                City = "Test City",
                State = "TS",
                Zip = "12345",
                Country = "USA"
            };

            // Assert
            entry.FacilityCode.Should().Be("FC123");
            entry.CardNumber.Should().Be("12345");
            entry.Name.Should().Be("Test Facility");
            entry.Address.Should().Be("123 Main St");
            entry.City.Should().Be("Test City");
            entry.State.Should().Be("TS");
            entry.Zip.Should().Be("12345");
            entry.Country.Should().Be("USA");
        }

        [Fact]
        public void AllProperties_CanBeNull()
        {
            // Act
            var entry = new RedIdRecordCSVEntry();

            // Assert
            entry.FacilityCode.Should().BeNull();
            entry.CardNumber.Should().BeNull();
            entry.Name.Should().BeNull();
            entry.Address.Should().BeNull();
            entry.City.Should().BeNull();
            entry.State.Should().BeNull();
            entry.Zip.Should().BeNull();
            entry.Country.Should().BeNull();
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
                Name = value3
            };

            // Assert
            entry.FacilityCode.Should().Be(value1);
            entry.CardNumber.Should().Be(value2);
            entry.Name.Should().Be(value3);
        }

        [Fact]
        public void CSVEntry_CanHandleLongStrings()
        {
            // Arrange
            var longString = new string('A', 1000);

            // Act
            var entry = new RedIdRecordCSVEntry
            {
                Name = longString,
                Address = longString
            };

            // Assert
            entry.Name.Should().HaveLength(1000);
            entry.Address.Should().HaveLength(1000);
        }

        [Fact]
        public void CSVEntry_CanHandleSpecialCharacters()
        {
            // Act
            var entry = new RedIdRecordCSVEntry
            {
                Name = "Test, Company & Associates",
                Address = "123 \"Main\" St.",
                City = "O'Brien"
            };

            // Assert
            entry.Name.Should().Contain(",");
            entry.Address.Should().Contain("\"");
            entry.City.Should().Contain("'");
        }
    }
}
