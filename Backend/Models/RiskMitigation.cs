using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skyworks.Backend.Models
{
    /// <summary>
    /// Στρατηγικές μετριασμού κινδύνου για UAS
    /// Risk Mitigation Strategies for UAS
    /// Υποστήριξη SORA 2.0 και 2.5
    /// </summary>
    public class RiskMitigation
    {
        /// <summary>
        /// Μοναδικό αναγνωριστικό μετριασμού
        /// Unique Mitigation Identifier
        /// </summary>
        [Required]
        public string MitigationId { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version (2.0 or 2.5)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SoraVersion { get; set; }

        /// <summary>
        /// Τύπος μετριασμού κινδύνου
        /// Risk Mitigation Type
        /// </summary>
        public RiskMitigationType Type { get; set; }

        /// <summary>
        /// Επίπεδο μείωσης κινδύνου
        /// Risk Reduction Level (0.0 - 1.0)
        /// </summary>
        [Range(0.0, 1.0)]
        public double ReductionFactor { get; set; }

        /// <summary>
        /// Απαιτούμενα αποδεικτικά
        /// Required Evidence
        /// </summary>
        public List<string> RequiredEvidence { get; set; }

        /// <summary>
        /// Επηρεαζόμενα OSOs
        /// Affected OSOs
        /// </summary>
        public List<string> AffectedOSOs { get; set; }

        /// <summary>
        /// Δημιουργία προεπιλεγμένων στρατηγικών μετριασμού
        /// Create default mitigation strategies
        /// </summary>
        public static List<RiskMitigation> GetDefaultStrategies(string soraVersion)
        {
            return soraVersion switch
            {
                "2.0" => CreateSora20Strategies(),
                "2.5" => CreateSora25Strategies(),
                _ => throw new ArgumentException($"Invalid SORA version: {soraVersion}")
            };
        }

        private static List<RiskMitigation> CreateSora20Strategies()
        {
            return new List<RiskMitigation>
            {
                new RiskMitigation
                {
                    MitigationId = "STRAT-GRC-01-20",
                    SoraVersion = "2.0",
                    Type = RiskMitigationType.GroundRisk,
                    ReductionFactor = 0.6,
                    RequiredEvidence = new List<string> 
                    { 
                        "Πιστοποιητικό εκπαίδευσης", 
                        "Σχέδιο διαχείρισης κινδύνου" 
                    },
                    AffectedOSOs = new List<string> { "OSO-10", "OSO-14", "OSO-15" }
                },
                new RiskMitigation
                {
                    MitigationId = "STRAT-ARC-01-20",
                    SoraVersion = "2.0",
                    Type = RiskMitigationType.AirRisk,
                    ReductionFactor = 0.5,
                    RequiredEvidence = new List<string> 
                    { 
                        "Πιστοποιητικό συστήματος D&A", 
                        "Αποδείξεις παρακολούθησης" 
                    },
                    AffectedOSOs = new List<string> { "OSO-11", "OSO-12" }
                }
            };
        }

        private static List<RiskMitigation> CreateSora25Strategies()
        {
            return new List<RiskMitigation>
            {
                new RiskMitigation
                {
                    MitigationId = "STRAT-GRC-01-25",
                    SoraVersion = "2.5",
                    Type = RiskMitigationType.GroundRisk,
                    ReductionFactor = 0.7,
                    RequiredEvidence = new List<string> 
                    { 
                        "Χάρτης περιοχής", 
                        "Στρατηγική μετριασμού εδαφικού κινδύνου" 
                    },
                    AffectedOSOs = new List<string> { "OSO-11", "OSO-17" }
                },
                new RiskMitigation
                {
                    MitigationId = "STRAT-ARC-01-25",
                    SoraVersion = "2.5",
                    Type = RiskMitigationType.AirRisk,
                    ReductionFactor = 0.6,
                    RequiredEvidence = new List<string> 
                    { 
                        "Πιστοποιητικό συστήματος ανίχνευσης", 
                        "Αποδεικτικά στρατηγικής αποφυγής" 
                    },
                    AffectedOSOs = new List<string> { "OSO-11", "OSO-23" }
                }
            };
        }
    }

    /// <summary>
    /// Τύποι μετριασμού κινδύνου
    /// Risk Mitigation Types
    /// </summary>
    public enum RiskMitigationType
    {
        GroundRisk,
        AirRisk,
        OperationalRisk
    }

    /// <summary>
    /// Αποτέλεσμα μετριασμού κινδύνου
    /// Risk Mitigation Result
    /// </summary>
    public class RiskMitigationResult
    {
        /// <summary>
        /// Αρχική τιμή κινδύνου
        /// Initial Risk Value
        /// </summary>
        public double InitialRisk { get; set; }

        /// <summary>
        /// Τελική τιμή κινδύνου
        /// Final Risk Value
        /// </summary>
        public double FinalRisk { get; set; }

        /// <summary>
        /// Παράγοντας μείωσης κινδύνου
        /// Risk Reduction Factor
        /// </summary>
        public double ReductionFactor { get; set; }

        /// <summary>
        /// Εφαρμοσμένες στρατηγικές
        /// Applied Strategies
        /// </summary>
        public List<string> AppliedStrategies { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version
        /// </summary>
        public string SoraVersion { get; set; }
    }
}