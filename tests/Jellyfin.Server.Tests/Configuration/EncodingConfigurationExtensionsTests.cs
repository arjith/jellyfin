using System;
using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Configuration
{
    public static class EncodingConfigurationExtensionsTests
    {
        [Fact]
        public static void GetTranscodePath_MissingCustomDirectory_ClaimsConfiguredPath()
        {
            var path = Path.Combine(Path.GetTempPath(), "jellyfin-transcode-tests", Guid.NewGuid().ToString("N"));
            var (configuration, applicationPaths) = CreateConfiguration(path);

            Assert.Equal(path, configuration.Object.GetTranscodePath());
            applicationPaths.Verify(p => p.CreateAndCheckMarker(path, "transcode", true), Times.Once);
        }

        [Fact]
        public static void GetTranscodePath_EmptyCustomDirectory_ClaimsConfiguredPath()
        {
            var path = CreateTempDirectory();
            try
            {
                var (configuration, applicationPaths) = CreateConfiguration(path);

                Assert.Equal(path, configuration.Object.GetTranscodePath());
                applicationPaths.Verify(p => p.CreateAndCheckMarker(path, "transcode", true), Times.Once);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void GetTranscodePath_NonEmptyUnmarkedCustomDirectory_ThrowsBeforeClaimingPath()
        {
            var path = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(path, "unrelated-user-file.txt"), "keep me");
                var (configuration, applicationPaths) = CreateConfiguration(path);

                Assert.Throws<InvalidOperationException>(() => configuration.Object.GetTranscodePath());
                applicationPaths.Verify(p => p.CreateAndCheckMarker(path, "transcode", true), Times.Never);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void GetTranscodePath_CustomDirectoryWithSubdirectory_ThrowsBeforeClaimingPath()
        {
            var path = CreateTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(path, "unrelated-user-directory"));
                var (configuration, applicationPaths) = CreateConfiguration(path);

                Assert.Throws<InvalidOperationException>(() => configuration.Object.GetTranscodePath());
                applicationPaths.Verify(p => p.CreateAndCheckMarker(path, "transcode", true), Times.Never);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void GetTranscodePath_MarkedCustomDirectoryWithContent_ReturnsConfiguredPath()
        {
            var path = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(path, ".jellyfin-transcode"), string.Empty);
                File.WriteAllText(Path.Combine(path, "existing-transcode.ts"), "data");
                var (configuration, applicationPaths) = CreateConfiguration(path);

                Assert.Equal(path, configuration.Object.GetTranscodePath());
                applicationPaths.Verify(p => p.CreateAndCheckMarker(path, "transcode", true), Times.Once);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void GetTranscodePath_DefaultPath_DoesNotRequirePreexistingMarker()
        {
            var cachePath = Path.Combine(Path.GetTempPath(), "jellyfin-transcode-tests", Guid.NewGuid().ToString("N"));
            var expectedPath = Path.Combine(cachePath, "transcodes");
            var (configuration, applicationPaths) = CreateConfiguration(null, cachePath);

            Assert.Equal(expectedPath, configuration.Object.GetTranscodePath());
            applicationPaths.Verify(p => p.CreateAndCheckMarker(expectedPath, "transcode", true), Times.Once);
        }

        private static (Mock<IConfigurationManager> Configuration, Mock<IApplicationPaths> ApplicationPaths) CreateConfiguration(
            string? transcodePath,
            string? cachePath = null)
        {
            var applicationPaths = new Mock<IApplicationPaths>();
            applicationPaths.SetupGet(p => p.CachePath).Returns(cachePath ?? Path.GetTempPath());

            var configuration = new Mock<IConfigurationManager>();
            configuration.Setup(c => c.GetConfiguration("encoding"))
                .Returns(new EncodingOptions { TranscodingTempPath = transcodePath });
            configuration.SetupGet(c => c.CommonApplicationPaths).Returns(applicationPaths.Object);

            return (configuration, applicationPaths);
        }

        private static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "jellyfin-transcode-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
