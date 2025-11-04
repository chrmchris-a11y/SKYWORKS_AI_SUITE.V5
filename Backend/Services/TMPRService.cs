using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;

namespace Skyworks.Backend.Services
{
    /// <summary>
    /// Υπηρεσία διαχείρισης Tactical Mitigation Performance Requirements (TMPR)
    /// Υποστήριξη EASA SORA 2.0 (24 OSOs) και JARUS SORA 2.5 (17 OSOs)
    /// </summary>
    public interface ITMPRService
    {
        /// <summary>
        /// Επικύρωση TMPR για συγκεκριμένο OSO
        /// Validation of Tactical Mitigation Performance Requirement
        /// </summary>
        /// <param name="operation">Στοιχεία επιχείρησης</param>
        /// <param name="osoId">Αναγνωριστικό OSO</param>
        /// <param name="soraVersion">Έκδοση SORA (2.0 ή 2.5)</param>
        /// <returns>Αποτέλεσμα επικύρωσης TMPR</returns>
        Task<TMPRValidationResult> ValidateTMPR(
            Operation operation, 
            string osoId, 
            string soraVersion
        );

        /// <summary>
        /// Λήψη διαθέσιμων συστημάτων TMPR
        /// Retrieval of available TMPR systems
        /// </summary>
        /// <param name="soraVersion">Έκδοση SORA (2.0 ή 2.5)</param>
        /// <returns>Λίστα συστημάτων TMPR</returns>
        List<TMPRSystem> GetAvailableTMPRSystems(string soraVersion);
    }

    /// <summary>
    /// Υλοποίηση υπηρεσίας TMPR
    /// TMPR Service Implementation
    /// </summary>
    public class TMPRService : ITMPRService
    {
        private readonly ILogger<TMPRService> _logger;

        public TMPRService(ILogger<TMPRService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Επικύρωση TMPR με βάση την έκδοση SORA
        /// TMPR Validation based on SORA version
        /// </summary>
        public async Task<TMPRValidationResult> ValidateTMPR(
            Operation operation, 
            string osoId, 
            string soraVersion)
        {
            _logger.LogInformation(
                "Validating TMPR for OSO {OsoId} in SORA {SoraVersion}", 
                osoId, soraVersion
            );

            // Προεπιλεγμένο αποτέλεσμα
            var result = new TMPRValidationResult
            {
                OsoId = osoId,
                SoraVersion = soraVersion,
                IsValid = false
            };

            try 
            {
                // Επιλογή λογικής επικύρωσης βάσει έκδοσης SORA
                result = soraVersion switch
                {
                    "2.0" => await ValidateTMPRForSora20(operation, osoId),
                    "2.5" => await ValidateTMPRForSora25(operation, osoId),
                    _ => throw new ArgumentException(
                        $"Μη έγκυρη έκδοση SORA: {soraVersion}", 
                        nameof(soraVersion)
                    )
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Σφάλμα επικύρωσης TMPR για OSO {OsoId}", 
                    osoId
                );
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Επικύρωση TMPR για SORA 2.0 (24 OSOs)
        /// TMPR Validation for SORA 2.0 (24 OSOs)
        /// </summary>
        private async Task<TMPRValidationResult> ValidateTMPRForSora20(
            Operation operation, 
            string osoId)
        {
            // Λίστα OSOs που απαιτούν TMPR στο SORA 2.0
            string[] tmprRequiredOsos20 = {
                "OSO-10", "OSO-11", "OSO-12", 
                "OSO-14", "OSO-15", "OSO-21", "OSO-22"
            };

            // Προεπιλογή αποτελέσματος
            var result = new TMPRValidationResult
            {
                OsoId = osoId,
                SoraVersion = "2.0",
                IsValid = false
            };

            // Έλεγχος εάν το OSO απαιτεί TMPR στο SORA 2.0
            if (!tmprRequiredOsos20.Contains(osoId))
            {
                result.IsValid = true;
                result.ValidationMessage = 
                    $"OSO {osoId} δεν απαιτεί TMPR στο SORA 2.0";
                return result;
            }

            // Εύρεση διαθέσιμου συστήματος TMPR
            var tmprSystems = operation.TMPRSystems
                .Where(t => t.ApplicableOSO == osoId)
                .ToList();

            // Αξιολόγηση συστημάτων TMPR
            if (tmprSystems.Any(t => t.ReliabilityFactor >= 0.9))
            {
                result.IsValid = true;
                result.ValidationMessage = 
                    $"Επιτυχής επικύρωση TMPR για OSO {osoId}";
            }
            else
            {
                result.ValidationMessage = 
                    $"Αποτυχία επικύρωσης TMPR για OSO {osoId}";
            }

            return result;
        }

        /// <summary>
        /// Επικύρωση TMPR για SORA 2.5 (17 OSOs)
        /// TMPR Validation for SORA 2.5 (17 OSOs)
        /// </summary>
        private async Task<TMPRValidationResult> ValidateTMPRForSora25(
            Operation operation, 
            string osoId)
        {
            // Λίστα OSOs που απαιτούν TMPR στο SORA 2.5
            string[] tmprRequiredOsos25 = {
                "OSO-11", "OSO-17", "OSO-23"
            };

            // Προεπιλογή αποτελέσματος
            var result = new TMPRValidationResult
            {
                OsoId = osoId,
                SoraVersion = "2.5",
                IsValid = false
            };

            // Έλεγχος εάν το OSO απαιτεί TMPR στο SORA 2.5
            if (!tmprRequiredOsos25.Contains(osoId))
            {
                result.IsValid = true;
                result.ValidationMessage = 
                    $"OSO {osoId} δεν απαιτεί TMPR στο SORA 2.5";
                return result;
            }

            // Εύρεση διαθέσιμου συστήματος TMPR
            var tmprSystems = operation.TMPRSystems
                .Where(t => t.ApplicableOSO == osoId)
                .ToList();

            // Αξιολόγηση συστημάτων TMPR
            if (tmprSystems.Any(t => t.ReliabilityFactor >= 0.95))
            {
                result.IsValid = true;
                result.ValidationMessage = 
                    $"Επιτυχής επικύρωση TMPR για OSO {osoId}";
            }
            else
            {
                result.ValidationMessage = 
                    $"Αποτυχία επικύρωσης TMPR για OSO {osoId}";
            }

            return result;
        }

        /// <summary>
        /// Λήψη διαθέσιμων συστημάτων TMPR
        /// Get available TMPR systems
        /// </summary>
        public List<TMPRSystem> GetAvailableTMPRSystems(string soraVersion)
        {
            // Προκαθορισμένα συστήματα TMPR
            var defaultTMPRSystems = new List<TMPRSystem>
            {
                new TMPRSystem 
                {
                    SystemType = "Parachute",
                    ReliabilityFactor = 0.95,
                    IsActiveForOSO = true,
                    ApplicableOSO = soraVersion == "2.0" ? "OSO-11" : "OSO-17"
                },
                new TMPRSystem 
                {
                    SystemType = "Flight Termination",
                    ReliabilityFactor = 0.92,
                    IsActiveForOSO = true,
                    ApplicableOSO = soraVersion == "2.0" ? "OSO-12" : "OSO-23"
                }
            };

            return defaultTMPRSystems;
        }
    }

    /// <summary>
    /// Αποτέλεσμα επικύρωσης TMPR
    /// TMPR Validation Result
    /// </summary>
    public class TMPRValidationResult
    {
        /// <summary>
        /// Αναγνωριστικό OSO
        /// OSO Identifier
        /// </summary>
        public string OsoId { get; set; }

        /// <summary>
        /// Έκδοση SORA
        /// SORA Version
        /// </summary>
        public string SoraVersion { get; set; }

        /// <summary>
        /// Κατάσταση επικύρωσης
        /// Validation Status
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Μήνυμα επικύρωσης
        /// Validation Message
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Μήνυμα σφάλματος (προαιρετικό)
        /// Error Message (optional)
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}