using System;
using System.IO;
using System.Linq;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Common.Configuration
{
    /// <summary>
    /// Class containing extension methods for working with the encoding configuration.
    /// </summary>
    public static class EncodingConfigurationExtensions
    {
        private const string TranscodeMarkerName = ".jellyfin-transcode";

        /// <summary>
        /// Gets the encoding options.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <returns>The encoding options.</returns>
        public static EncodingOptions GetEncodingOptions(this IConfigurationManager configurationManager)
            => configurationManager.GetConfiguration<EncodingOptions>("encoding");

        /// <summary>
        /// Retrieves the transcoding temp path from the encoding configuration, falling back to a default if no path
        /// is specified in configuration. If the directory does not exist, it will be created.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <returns>The transcoding temp path.</returns>
        /// <exception cref="UnauthorizedAccessException">If the directory does not exist, and the caller does not have the required permission to create it.</exception>
        /// <exception cref="NotSupportedException">If there is a custom path transcoding path specified, but it is invalid.</exception>
        /// <exception cref="IOException">If the directory does not exist, and it also could not be created.</exception>
        /// <exception cref="InvalidOperationException">If the configured custom path is non-empty and is not already owned by Jellyfin.</exception>
        public static string GetTranscodePath(this IConfigurationManager configurationManager)
        {
            // Get the configured path and fall back to a default
            var transcodingTempPath = configurationManager.GetEncodingOptions().TranscodingTempPath;
            var isCustomPath = !string.IsNullOrEmpty(transcodingTempPath);
            if (!isCustomPath)
            {
                transcodingTempPath = Path.Combine(configurationManager.CommonApplicationPaths.CachePath, "transcodes");
            }

            if (isCustomPath && !CanClaimTranscodeDirectory(transcodingTempPath))
            {
                throw new InvalidOperationException(
                    $"Refusing to use non-empty transcode directory '{transcodingTempPath}' because it is not marked as owned by Jellyfin.");
            }

            configurationManager.CommonApplicationPaths.CreateAndCheckMarker(transcodingTempPath, "transcode", true);
            return transcodingTempPath;
        }

        private static bool CanClaimTranscodeDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return true;
            }

            if (File.Exists(Path.Combine(path, TranscodeMarkerName)))
            {
                return true;
            }

            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
