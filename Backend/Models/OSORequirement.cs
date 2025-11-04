using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Μοντέλο δεδομένων για OSO (Operational Safety Objective) requirements
    /// Υποστήριξη EASA SORA 2.0 (24 OSOs) και JARUS SORA 2.5 (17 OSOs)
    /// </summary>
    public class OSORequirement
    {
        /// <summary>
        /// Αναγνωριστικό OSO (π.χ. "OSO-11", "OSO-17")
        /// </summary>
        [Required]
        public string OSOId { get; set; } = string.Empty;

        /// <summary>
        /// Έκδοση SORA ("2.0" ή "2.5")
        /// SORA Version ("2.0" or "2.5")
        /// </summary>
        [Required]
        public string SoraVersion { get; set; } = string.Empty;

        /// <summary>
        /// Περιγραφή του OSO
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Κατηγορία OSO (Strategic, Tactical, Human Factors, Technical)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Ελάχιστο SAIL level που απαιτεί αυτό το OSO
        /// </summary>
        public int MinimumSAIL { get; set; }

        /// <summary>
        /// Επίπεδο συμμόρφωσης με τις απαιτήσεις OSO (1-5)
        /// </summary>
        public int RobustnessLevel { get; set; } = 1;

        /// <summary>
        /// Μέτρα μετριασμού (Mitigation Measures)
        /// </summary>
        public List<string> MitigationMeasures { get; set; } = new List<string>();
        /// <summary>
        /// Εάν εφαρμόζεται ποινή M3 (Tactical Mitigation Performance Requirement)
        /// </summary>
        public bool M3PenaltyApplies { get; set; }
    /// <summary>
        /// Κατασκευή προκαθορισμένων OSO για τα βασικά σημεία ελέγχου
        /// Construction of predefined OSOs for key validation points
        /// </summary>
        /// <param name="osoId">OSO identifier</param>
        /// <param name="soraVersion">SORA version ("2.0" or "2.5")</param>
        /// <returns>Configured OSO requirement</returns>
        public static OSORequirement CreateDefaultOSO(string osoId, string soraVersion)
        {
            SoraVersionValidator.ValidateVersion(soraVersion);
            
            return osoId switch
    {
                "OSO-11" => new OSORequirement
    {
                    OSOId = "OSO-11",
                    SoraVersion = soraVersion,
                    Description = soraVersion == "2.0" ? "Detect & Avoid (Strategic/Tactical) - EASA AMC" : "Detect & Avoid (Simplified) - JARUS",
                    Category = "Strategic/Tactical",
                    MinimumSAIL = soraVersion == "2.0" ? 2 : 3,
                    MitigationMeasures = soraVersion == "2.0" ? new List<string>
                    {
                        "Pre-flight planning",
                        "Active D&A system",
                        "Visual observers",
                        "Tactical collision avoidance"
                    } : new List<string>
                    {
                        "Strategic avoidance protocol",
                        "See-and-avoid procedures",
                        "Visual observer competency"
                    }
                },
                "OSO-17" => new OSORequirement
    {
                    OSOId = "OSO-17",
                    SoraVersion = soraVersion,
                    Description = soraVersion == "2.0" ? "Operational Volume Management - 3D Envelope" : "Operational Volume Management - Simplified",
                    Category = "Technical",
                    MinimumSAIL = soraVersion == "2.0" ? 1 : 2,
                    MitigationMeasures = soraVersion == "2.0" ? new List<string>
                    {
                        "3D volume calculations",
                        "GPS/GNSS accuracy buffers",
                        "Emergency containment zones",
                        "Safety margin validation"
                    } : new List<string>
                    {
                        "2D operational boundaries",
                        "Horizontal containment",
                        "Altitude limits",
                        "Contingency areas"
                    }
                },
                "OSO-19" => new OSORequirement
    {
                    OSOId = "OSO-19",
                    SoraVersion = soraVersion,
                    Description = soraVersion == "2.0" ? "Ground Risk Mitigation - Comprehensive" : "Ground Risk Mitigation - Strategic",
                    Category = "Strategic",
                    MinimumSAIL = soraVersion == "2.0" ? 2 : 3,
                    MitigationMeasures = soraVersion == "2.0" ? new List<string>
                    {
                        "10m resolution ground risk models",
                        "Population density maps",
                        "Debris dispersion analysis",
                        "Shelter factor calculations"
                    } : new List<string>
                    {
                        "Basic population analysis",
                        "High-density area focus",
                        "Emergency landing zones",
                        "Simplified risk categories"
                    }
                },
                "OSO-23" => new OSORequirement
    {
                    OSOId = "OSO-23",
                    SoraVersion = soraVersion,
                    Description = soraVersion == "2.0" ? "External Services Monitoring - Comprehensive" : "External Services Monitoring - Basic",
                    Category = "Technical",
                    MinimumSAIL = soraVersion == "2.0" ? 2 : 3,
                    MitigationMeasures = soraVersion == "2.0" ? new List<string>
                    {
                        "GPS/GNSS signal quality monitoring",
                        "Service redundancy implementation",
                        "Continuous health monitoring",
                        "Service loss procedures"
                    } : new List<string>
                    {
                        "Critical service dependency tracking",
                        "Manual override procedures",
                        "Service failure response plans",
                        "Service loss scenario testing"
                    }
                },
                _ when !SoraVersionConstants.GetOSOsForVersion(soraVersion).Contains(osoId) => 
                    throw new ArgumentException($"OSO {osoId} not available in SORA {soraVersion}"),
                _ => throw new ArgumentException($"OSO {osoId} not implemented for version {soraVersion}")
            };
        }
    }

    // Rest of the code remains the same
}
