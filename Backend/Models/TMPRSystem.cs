using System;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Σύστημα Τακτικού Μετριασμού Επιδόσεων (TMPR)
    /// Tactical Mitigation Performance Requirement System
    /// Υποστηρίζει EASA SORA 2.0 και JARUS SORA 2.5
    /// </summary>
    public class TMPRSystem
    {
        /// <summary>
        /// Τύπος συστήματος TMPR
        /// TMPR System Type
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SystemType { get; set; } = string.Empty;

        /// <summary>
        /// Παράγοντας αξιοπιστίας συστήματος
        /// System Reliability Factor (0.0 - 1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double ReliabilityFactor { get; set; }

        /// <summary>
        /// Κατάσταση ενεργοποίησης για συγκεκριμένο OSO
        /// Active status for specific OSO
        /// </summary>
        public bool IsActiveForOSO { get; set; }

        /// <summary>
        /// Εφαρμοζόμενο OSO
        /// Applicable Operational Safety Objective
        /// </summary>
        [Required]
        [StringLength(10)]
        public string ApplicableOSO { get; set; } = string.Empty;

        /// <summary>
        /// Περιγραφή συστήματος TMPR
        /// TMPR System Description
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ημερομηνία πιστοποίησης
        /// Certification Date
        /// </summary>
        public DateTime CertificationDate { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version (2.0 or 2.5)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SoraVersion { get; set; } = "2.5";

        /// <summary>
        /// Δημιουργία προεπιλεγμένων συστημάτων TMPR
        /// Create default TMPR systems
        /// </summary>
        public static TMPRSystem CreateDefaultSystem(string systemType, string soraVersion)
        {
            return new TMPRSystem
            {
                SystemType = systemType,
                ReliabilityFactor = soraVersion == "2.0" ? 0.90 : 0.95,
                IsActiveForOSO = true,
                CertificationDate = DateTime.UtcNow,
                SoraVersion = soraVersion,
                Description = GetDefaultDescription(systemType, soraVersion)
            };
        }

        /// <summary>
        /// Λήψη προεπιλεγμένης περιγραφής
        /// Get default description
        /// </summary>
        private static string GetDefaultDescription(string systemType, string soraVersion)
        {
            return soraVersion switch
            {
                "2.0" => systemType switch
                {
                    "Parachute" => "Σύστημα αλεξιπτώτου για μετριασμό κινδύνου (SORA 2.0)",
                    "Flight Termination" => "Σύστημα τερματισμού πτήσης (SORA 2.0)",
                    _ => "Προεπιλεγμένο σύστημα TMPR"
                },
                "2.5" => systemType switch
                {
                    "Parachute" => "Σύστημα αλεξιπτώτου με βελτιωμένη αξιοπιστία (SORA 2.5)",
                    "Flight Termination" => "Προηγμένο σύστημα τερματισμού πτήσης (SORA 2.5)",
                    _ => "Σύστημα TMPR SORA 2.5"
                },
                _ => "Σύστημα TMPR"
            };
        }
    }
}