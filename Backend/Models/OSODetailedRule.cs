using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Λεπτομερείς κανόνες Operational Safety Objectives (OSO)
    /// Υποστήριξη EASA SORA 2.0 (24 OSOs) και JARUS SORA 2.5 (17 OSOs)
    /// </summary>
    public class OSODetailedRule
    {
        /// <summary>
        /// Μοναδικό αναγνωριστικό OSO
        /// Unique OSO Identifier
        /// </summary>
        [Required]
        [StringLength(10)]
        public string OsoId { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version (2.0 or 2.5)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SoraVersion { get; set; }

        /// <summary>
        /// Περιγραφή του OSO
        /// OSO Description
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Επίπεδο ισχύος (Robustness Level)
        /// Robustness Level (1-5)
        /// </summary>
        [Range(1, 5)]
        public int RobustnessLevel { get; set; }

        /// <summary>
        /// Απαιτούμενα επίπεδα SAIL
        /// Required SAIL Levels
        /// </summary>
        public SAILRequirements SAILRequirements { get; set; }

        /// <summary>
        /// Συνθήκες συμμόρφωσης
        /// Compliance Conditions
        /// </summary>
        public List<ComplianceCondition> ComplianceConditions { get; set; }

        /// <summary>
        /// Απαιτούμενα αποδεικτικά
        /// Required Evidence
        /// </summary>
        public List<string> RequiredEvidence { get; set; }

        /// <summary>
        /// Κατηγοριοποίηση κινδύνου
        /// Risk Classification
        /// </summary>
        public RiskClassification RiskClassification { get; set; }

        /// <summary>
        /// Δημιουργία προεπιλεγμένου κανόνα OSO
        /// Create default OSO rule
        /// </summary>
        public static OSODetailedRule CreateDefaultRule(string osoId, string soraVersion)
        {
            return soraVersion switch
            {
                "2.0" => CreateSora20Rule(osoId),
                "2.5" => CreateSora25Rule(osoId),
                _ => throw new ArgumentException($"Invalid SORA version: {soraVersion}")
            };
        }

        private static OSODetailedRule CreateSora20Rule(string osoId)
        {
            return osoId switch
            {
                "OSO-10" => new OSODetailedRule
                {
                    OsoId = "OSO-10",
                    SoraVersion = "2.0",
                    Description = "Προ-πτητικός σχεδιασμός και προετοιμασία",
                    RobustnessLevel = 3,
                    SAILRequirements = new SAILRequirements { MinSAIL = 1, MaxSAIL = 6 },
                    RequiredEvidence = new List<string> 
                    { 
                        "Έγγραφο προ-πτητικού σχεδιασμού", 
                        "Λίστα ελέγχου προετοιμασίας" 
                    }
                },
                "OSO-11" => new OSODetailedRule
                {
                    OsoId = "OSO-11",
                    SoraVersion = "2.0",
                    Description = "Στρατηγική και τακτική ανίχνευση και αποφυγή",
                    RobustnessLevel = 4,
                    SAILRequirements = new SAILRequirements { MinSAIL = 2, MaxSAIL = 6 },
                    RequiredEvidence = new List<string> 
                    { 
                        "Πιστοποιητικό συστήματος D&A", 
                        "Αποδείξεις εκπαίδευσης οπτικών παρατηρητών" 
                    }
                },
                // Additional SORA 2.0 OSO rules can be added here
                _ => throw new ArgumentException($"Unknown OSO for SORA 2.0: {osoId}")
            };
        }

        private static OSODetailedRule CreateSora25Rule(string osoId)
        {
            return osoId switch
            {
                "OSO-11" => new OSODetailedRule
                {
                    OsoId = "OSO-11",
                    SoraVersion = "2.5",
                    Description = "Ανίχνευση και αποφυγή",
                    RobustnessLevel = 3,
                    SAILRequirements = new SAILRequirements { MinSAIL = 2, MaxSAIL = 5 },
                    RequiredEvidence = new List<string> 
                    { 
                        "Πιστοποιητικό συστήματος D&A", 
                        "Στρατηγική αποφυγής σύγκρουσης" 
                    }
                },
                "OSO-17" => new OSODetailedRule
                {
                    OsoId = "OSO-17",
                    SoraVersion = "2.5",
                    Description = "Ορισμός επιχειρησιακού όγκου",
                    RobustnessLevel = 2,
                    SAILRequirements = new SAILRequirements { MinSAIL = 1, MaxSAIL = 4 },
                    RequiredEvidence = new List<string> 
                    { 
                        "Χάρτης επιχειρησιακού όγκου", 
                        "Υπολογισμοί περιθωρίου ασφαλείας" 
                    }
                },
                // Additional SORA 2.5 OSO rules can be added here
                _ => throw new ArgumentException($"Unknown OSO for SORA 2.5: {osoId}")
            };
        }
    }

    /// <summary>
    /// Απαιτήσεις επιπέδων SAIL
    /// SAIL Level Requirements
    /// </summary>
    public class SAILRequirements
    {
        /// <summary>
        /// Ελάχιστο επίπεδο SAIL
        /// Minimum SAIL Level
        /// </summary>
        [Range(0, 6)]
        public int MinSAIL { get; set; }

        /// <summary>
        /// Μέγιστο επίπεδο SAIL
        /// Maximum SAIL Level
        /// </summary>
        [Range(0, 6)]
        public int MaxSAIL { get; set; }
    }

    /// <summary>
    /// Συνθήκη συμμόρφωσης
    /// Compliance Condition
    /// </summary>
    public class ComplianceCondition
    {
        /// <summary>
        /// Περιγραφή συνθήκης
        /// Condition Description
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Κανόνας επαλήθευσης
        /// Validation Rule
        /// </summary>
        [Required]
        public Func<object, bool> ValidationRule { get; set; }
    }

    /// <summary>
    /// Κατηγοριοποίηση κινδύνου
    /// Risk Classification
    /// </summary>
    public class RiskClassification
    {
        /// <summary>
        /// Επίπεδο κινδύνου
        /// Risk Level
        /// </summary>
        public string RiskLevel { get; set; }

        /// <summary>
        /// Περιγραφή κινδύνου
        /// Risk Description
        /// </summary>
        public string Description { get; set; }
    }
}