using FluentAssertions;
using WebAppExperimental26.Models.Settings;

namespace WebAppExperimental26.Tests.Models
{
    /// <summary>
    /// Unit tests for FeatureFlags configuration model — security fix #8.
    /// </summary>
    public class FeatureFlagsTests
    {
        /// <summary>
        /// Security fix #8: authentication and authorization must default to enabled so that a
        /// developer who creates a new appsettings file without explicitly setting these flags
        /// does not accidentally deploy an open application.
        /// This test fails if either default is changed back to false.
        /// </summary>
        [Fact]
        public void DefaultFlags_HaveAuthenticationAndAuthorizationEnabled()
        {
            // Act
            var flags = new FeatureFlags();

            // Assert
            flags.EnableAzureAd.Should().BeTrue(
                "authentication must be ON by default — opt-out is safer than opt-in (security fix #8)");
            flags.EnableAuthorization.Should().BeTrue(
                "authorization must be ON by default — opt-out is safer than opt-in (security fix #8)");
        }

        [Fact]
        public void DefaultFlags_EnableSessionAndLocalization()
        {
            var flags = new FeatureFlags();

            flags.EnableSession.Should().BeTrue();
            flags.EnableLocalization.Should().BeTrue();
        }

        [Fact]
        public void DefaultFlags_EnableSecurityHeaders()
        {
            var flags = new FeatureFlags();

            flags.EnableSecurityHeaders.Should().BeTrue();
        }

        [Fact]
        public void AllFlags_CanBeOverridden()
        {
            // Arrange — simulate an explicit opt-out (e.g. integration test environment)
            var flags = new FeatureFlags
            {
                EnableAzureAd = false,
                EnableAuthorization = false
            };

            // Assert — overrides still work when intentional
            flags.EnableAzureAd.Should().BeFalse();
            flags.EnableAuthorization.Should().BeFalse();
        }
    }
}
