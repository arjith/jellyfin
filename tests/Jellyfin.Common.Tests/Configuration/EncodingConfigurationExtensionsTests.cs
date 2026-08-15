using System;
using System.IO;
using MediaBrowser.Common.Configuration;
using Xunit;

namespace Jellyfin.Common.Tests.Configuration
{
    public static class EncodingConfigurationExtensionsTests
    {
        [Fact]
        public static void CanClaimTranscodeDirectory_MissingDirectory_ReturnsTrue()
        {
            var path = Path.Combine(Path.GetTempPath(), "jellyfin-common-tests", Guid.NewGuid().ToString("N"));

            Assert.True(EncodingConfigurationExtensions.CanClaimTranscodeDirectory(path));
        }

        [Fact]
        public static void CanClaimTranscodeDirectory_EmptyDirectory_ReturnsTrue()
        {
            var path = CreateTempDirectory();
            try
            {
                Assert.True(EncodingConfigurationExtensions.CanClaimTranscodeDirectory(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void CanClaimTranscodeDirectory_NonEmptyUnmarkedDirectory_ReturnsFalse()
        {
            var path = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(path, "unrelated-user-file.txt"), "keep me");

                Assert.False(EncodingConfigurationExtensions.CanClaimTranscodeDirectory(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void CanClaimTranscodeDirectory_UnmarkedDirectoryWithSubdirectory_ReturnsFalse()
        {
            var path = CreateTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(path, "unrelated-user-directory"));

                Assert.False(EncodingConfigurationExtensions.CanClaimTranscodeDirectory(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [Fact]
        public static void CanClaimTranscodeDirectory_MarkedDirectoryWithOtherContent_ReturnsTrue()
        {
            var path = CreateTempDirectory();
            try
            {
                File.WriteAllText(Path.Combine(path, ".jellyfin-transcode"), string.Empty);
                File.WriteAllText(Path.Combine(path, "existing-transcode.ts"), "data");

                Assert.True(EncodingConfigurationExtensions.CanClaimTranscodeDirectory(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        private static string CreateTempDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "jellyfin-common-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
