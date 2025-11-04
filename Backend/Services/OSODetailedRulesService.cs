using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;

namespace Skyworks.Backend.Services
{
    public interface IOSODetailedRulesService
    {
        OSODetailedRule GetOSORule(string osoId, string soraVersion);
        List<string> ValidateOSOCompliance(Operation operation, OSODetailedRule rule);
        OSOComplianceReport GenerateComplianceReport(Operation operation);
    }

    public class OSODetailedRulesService : IOSODetailedRulesService
    {
        private readonly ILogger<OSODetailedRulesService> _logger;

        public OSODetailedRulesService(ILogger<OSODetailedRulesService> logger)
        {
            _logger = logger;
        }

        public OSODetailedRule GetOSORule(string osoId, string soraVersion)
        {
            try
            {
                return OSODetailedRule.CreateDefaultRule(osoId, soraVersion);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid OSO or SORA version");
                throw;
            }
        }

        public List<string> ValidateOSOCompliance(Operation operation, OSODetailedRule rule)
        {
            var complianceIssues = new List<string>();

            // SAIL level check
            if (operation.SAIL < rule.SAILRequirements.MinSAIL || 
                operation.SAIL > rule.SAILRequirements.MaxSAIL)
            {
                complianceIssues.Add($"SAIL level {operation.SAIL} outside allowed range");
            }

            // Evidence check
            var missingEvidence = rule.RequiredEvidence
                .Where(evidence => !operation.OSORequirements
                    .Any(req => req.RequiredEvidence.Contains(evidence)))
                .ToList();

            complianceIssues.AddRange(
                missingEvidence.Select(e => $"Missing required evidence: {e}")
            );

            return complianceIssues;
        }

        public OSOComplianceReport GenerateComplianceReport(Operation operation)
        {
            var report = new OSOComplianceReport
            {
                OperationId = operation.OperationId,
                SoraVersion = operation.SoraVersion
            };

            var applicableOSOs = operation.SoraVersion == "2.0" 
                ? new[] { "OSO-10", "OSO-11", "OSO-12", "OSO-14", "OSO-15", "OSO-21", "OSO-22" }
                : new[] { "OSO-11", "OSO-17", "OSO-23" };

            foreach (var osoId in applicableOSOs)
            {
                try
                {
                    var rule = GetOSORule(osoId, operation.SoraVersion);
                    var complianceIssues = ValidateOSOCompliance(operation, rule);

                    report.OSOResults.Add(new OSOComplianceResult
                    {
                        OsoId = osoId,
                        IsCompliant = !complianceIssues.Any(),
                        ComplianceIssues = complianceIssues
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing {osoId}");
                }
            }

            return report;
        }
    }

    public class OSOComplianceReport
    {
        public string OperationId { get; set; }
        public string SoraVersion { get; set; }
        public List<OSOComplianceResult> OSOResults { get; set; } = new List<OSOComplianceResult>();
        public bool IsFullyCompliant => OSOResults.All(r => r.IsCompliant);
    }

    public class OSOComplianceResult
    {
        public string OsoId { get; set; }
        public bool IsCompliant { get; set; }
        public List<string> ComplianceIssues { get; set; } = new List<string>();
    }
}