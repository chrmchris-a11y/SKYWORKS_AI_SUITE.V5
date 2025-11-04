using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Μοντέλο επιχειρησιακής λειτουργίας UAS
    /// UAS Operation Model
    /// Υποστηρίζει EASA SORA 2.0 και JARUS SORA 2.5
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Μοναδικό αναγνωριστικό επιχείρησης
        /// Unique Operation Identifier
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Όνομα φορέα εκμετάλλευσης
        /// Operator Name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string OperatorName { get; set; } = string.Empty;

        /// <summary>
        /// Επίπεδο Safety Assurance Integrity Level (SAIL)
        /// Safety Assurance Integrity Level
        /// </summary>
        [Range(0, 6)]
        public int SAIL { get; set; }

        /// <summary>
        /// Επίπεδο Air Risk Class (ARC)
        /// Air Risk Class
        /// </summary>
        [Required]
        [StringLength(10)]
        public string ARC { get; set; } = "ARC-a";

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version (2.0 or 2.5)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SoraVersion { get; set; } = "2.5";

        /// <summary>
        /// Συστήματα Tactical Mitigation Performance Requirement (TMPR)
        /// Tactical Mitigation Performance Requirement Systems
        /// </summary>
        public List<TMPRSystem> TMPRSystems { get; set; } = new();

        /// <summary>
        /// Απαιτήσεις Operational Safety Objectives (OSO)
        /// Operational Safety Objectives Requirements
        /// </summary>
        public List<OSORequirement> OSORequirements { get; set; } = new();

        /// <summary>
        /// Δημιουργία προεπιλεγμένης επιχείρησης
        /// Create default operation
        /// </summary>
        public static Operation CreateDefaultOperation(string soraVersion)
        {
            var operation = new Operation
            {
                OperationId = $"OP-{Guid.NewGuid():N}",
                OperatorName = "Default Operator",
                SAIL = 3,
                ARC = "ARC-b",
                SoraVersion = soraVersion
            };

            // Προσθήκη προεπιλεγμένων συστημάτων TMPR
            operation.TMPRSystems.AddRange(new[]
            {
                TMPRSystem.CreateDefaultSystem("Parachute", soraVersion),
                TMPRSystem.CreateDefaultSystem("Flight Termination", soraVersion)
            });

            // Προσθήκη προεπιλεγμένων απαιτήσεων OSO
            operation.OSORequirements.AddRange(
                soraVersion == "2.0" 
                    ? GetDefaultOSORequirementsSora20() 
                    : GetDefaultOSORequirementsSora25()
            );

            return operation;
        }

        /// <summary>
        /// Λήψη προεπιλεγμένων απαιτήσεων OSO για SORA 2.0
        /// Get default OSO requirements for SORA 2.0
        /// </summary>
        private static List<OSORequirement> GetDefaultOSORequirementsSora20()
        {
            return new List<OSORequirement>
            {
                OSORequirement.CreateDefaultOSO("OSO-10"),
                OSORequirement.CreateDefaultOSO("OSO-11"),
                OSORequirement.CreateDefaultOSO("OSO-12"),
                OSORequirement.CreateDefaultOSO("OSO-14"),
                OSORequirement.CreateDefaultOSO("OSO-15"),
                OSORequirement.CreateDefaultOSO("OSO-21"),
                OSORequirement.CreateDefaultOSO("OSO-22")
            };
        }

        /// <summary>
        /// Λήψη προεπιλεγμένων απαιτήσεων OSO για SORA 2.5
        /// Get default OSO requirements for SORA 2.5
        /// </summary>
        private static List<OSORequirement> GetDefaultOSORequirementsSora25()
        {
            return new List<OSORequirement>
            {
                OSORequirement.CreateDefaultOSO("OSO-11"),
                OSORequirement.CreateDefaultOSO("OSO-17"),
                OSORequirement.CreateDefaultOSO("OSO-23")
            };
        }
    }
}