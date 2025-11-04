using System;
using Skyworks.Core.Models.GRC;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Validator for SORA version strings
    /// Ensures consistency across all services and endpoints
    /// </summary>
    public static class SoraVersionValidator
    {
        /// <summary>
        /// Supported SORA versions
        /// </summary>
        public static readonly string[] SupportedVersions = { "2.0", "2.5" };

        /// <summary>
        /// Check if a SORA version is valid
        /// </summary>
        /// <param name="soraVersion">Version to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidVersion(string soraVersion)
        {
            if (string.IsNullOrWhiteSpace(soraVersion))
                return false;

            return soraVersion == "2.0" || soraVersion == "2.5";
        }

        /// <summary>
        /// Try to parse a SORA version string to the Core enum
        /// </summary>
        /// <param name="soraVersion">Input version string (e.g., "2.0" or "2.5")</param>
        /// <param name="versionEnum">Parsed enum value if successful</param>
        /// <returns>True if parsed successfully, otherwise false</returns>
        public static bool TryParse(string soraVersion, out SoraVersion versionEnum)
        {
            versionEnum = default;
            if (!IsValidVersion(soraVersion))
                return false;

            versionEnum = soraVersion == "2.0" ? SoraVersion.SORA_2_0_AMC : SoraVersion.SORA_2_5;
            return true;
        }

        /// <summary>
        /// Convert a SORA version enum to its canonical string ("2.0" or "2.5")
        /// </summary>
        public static string ToString(SoraVersion versionEnum)
        {
            return versionEnum == SoraVersion.SORA_2_0_AMC ? "2.0" : "2.5";
        }

        /// <summary>
        /// Validate SORA version and throw exception if invalid
        /// </summary>
        /// <param name="soraVersion">Version to validate</param>
        /// <exception cref="ArgumentException">Thrown when version is invalid</exception>
        public static void ValidateVersion(string soraVersion)
        {
            if (!IsValidVersion(soraVersion))
            {
                throw new ArgumentException(
                    $"Invalid SORA version: '{soraVersion}'. Supported versions: {string.Join(", ", SupportedVersions)}",
                    nameof(soraVersion)
                );
            }
        }

        /// <summary>
        /// Get the default SORA version (latest)
        /// </summary>
        /// <returns>Default version string</returns>
        public static string GetDefaultVersion()
        {
            return "2.5";
        }

        /// <summary>
        /// Get the default SORA version as enum (latest)
        /// </summary>
        public static SoraVersion GetDefaultVersionEnum()
        {
            return SoraVersion.SORA_2_5;
        }
    }
}