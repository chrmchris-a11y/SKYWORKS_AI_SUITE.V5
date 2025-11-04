namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Version-specific constants for SORA 2.0 and 2.5 implementations
    /// Used to ensure consistent behavior across all services
    /// </summary>
    public static class SoraVersionConstants
    {
        /// <summary>
        /// SORA 2.0 (EASA AMC) specific constants
        /// </summary>
        public static class Sora20
        {
            public const int TotalOSOs = 24;
            public const double TMPRReliabilityThreshold = 0.90;
            
            /// <summary>
            /// OSOs that require TMPR validation in SORA 2.0
            /// </summary>
            public static readonly string[] TMPRRequiredOSOs = 
            {
                "OSO-10", "OSO-11", "OSO-12", 
                "OSO-14", "OSO-15", "OSO-21", "OSO-22"
            };

            /// <summary>
            /// All OSOs available in SORA 2.0
            /// </summary>
            public static readonly string[] AllOSOs = 
            {
                "OSO-01", "OSO-02", "OSO-03", "OSO-04", "OSO-05",
                "OSO-06", "OSO-07", "OSO-08", "OSO-09", "OSO-10",
                "OSO-11", "OSO-12", "OSO-13", "OSO-14", "OSO-15",
                "OSO-16", "OSO-17", "OSO-18", "OSO-19", "OSO-20",
                "OSO-21", "OSO-22", "OSO-23", "OSO-24"
            };

            /// <summary>
            /// Risk mitigation matrix identifier for SORA 2.0
            /// </summary>
            public const string RiskMitigationMatrix = "EASA_AMC_UAS_SORA_10_v2.0";
        }

        /// <summary>
        /// SORA 2.5 (JARUS) specific constants
        /// </summary>
        public static class Sora25
        {
            public const int TotalOSOs = 17;
            public const double TMPRReliabilityThreshold = 0.95;
            
            /// <summary>
            /// OSOs that require TMPR validation in SORA 2.5
            /// </summary>
            public static readonly string[] TMPRRequiredOSOs = 
            {
                "OSO-11", "OSO-17", "OSO-23"
            };

            /// <summary>
            /// All OSOs available in SORA 2.5
            /// </summary>
            public static readonly string[] AllOSOs = 
            {
                "OSO-01", "OSO-02", "OSO-03", "OSO-04", "OSO-05",
                "OSO-06", "OSO-07", "OSO-08", "OSO-09", "OSO-10",
                "OSO-11", "OSO-12", "OSO-13", "OSO-14", "OSO-15",
                "OSO-16", "OSO-17"
            };

            /// <summary>
            /// Risk mitigation matrix identifier for SORA 2.5
            /// </summary>
            public const string RiskMitigationMatrix = "JARUS_SORA_v2.5";
        }

        /// <summary>
        /// Get OSO list for specified version
        /// </summary>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Array of OSO identifiers</returns>
        public static string[] GetOSOsForVersion(string soraVersion)
        {
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            return soraVersion switch
            {
                "2.0" => Sora20.AllOSOs,
                "2.5" => Sora25.AllOSOs,
                _ => throw new System.ArgumentException($"Unknown SORA version: {soraVersion}")
            };
        }

        /// <summary>
        /// Get TMPR required OSOs for specified version
        /// </summary>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Array of OSO identifiers that require TMPR</returns>
        public static string[] GetTMPRRequiredOSOs(string soraVersion)
        {
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            return soraVersion switch
            {
                "2.0" => Sora20.TMPRRequiredOSOs,
                "2.5" => Sora25.TMPRRequiredOSOs,
                _ => throw new System.ArgumentException($"Unknown SORA version: {soraVersion}")
            };
        }

        /// <summary>
        /// Get TMPR reliability threshold for specified version
        /// </summary>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Minimum reliability threshold</returns>
        public static double GetTMPRReliabilityThreshold(string soraVersion)
        {
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            return soraVersion switch
            {
                "2.0" => Sora20.TMPRReliabilityThreshold,
                "2.5" => Sora25.TMPRReliabilityThreshold,
                _ => throw new System.ArgumentException($"Unknown SORA version: {soraVersion}")
            };
        }

        /// <summary>
        /// Get risk mitigation matrix identifier for specified version
        /// </summary>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Risk mitigation matrix identifier</returns>
        public static string GetRiskMitigationMatrix(string soraVersion)
        {
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            return soraVersion switch
            {
                "2.0" => Sora20.RiskMitigationMatrix,
                "2.5" => Sora25.RiskMitigationMatrix,
                _ => throw new System.ArgumentException($"Unknown SORA version: {soraVersion}")
            };
        }
    }
}