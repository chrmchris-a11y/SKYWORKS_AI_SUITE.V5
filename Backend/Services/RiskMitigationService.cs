using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;

namespace Skyworks.Backend.Services
{
    public interface IRiskMitigationService
    {
        RiskMitigationResult MitigateGroundRisk(Operation operation);
        RiskMitigationResult MitigateAirRisk(Operation operation);
        int ApplyFloorRule(Operation operation);
        int RecalculateSAIL(Operation operation);
    }

    public class RiskMitigationService : IRiskMitigationService
    {
        private readonly ILogger<RiskMitigationService> _logger;

        public RiskMitigationService(ILogger<RiskMitigationService> logger)
        {
            _logger = logger;
        }

        public RiskMitigationResult MitigateGroundRisk(Operation operation)
        {
            var strategies = RiskMitigation.GetDefaultStrategies(operation.SoraVersion)
                .Where(s => s.Type == RiskMitigationType.GroundRisk)
                .ToList();

            var result = new RiskMitigationResult
            {
                InitialRisk = CalculateInitialGroundRisk(operation),
                SoraVersion = operation.SoraVersion,
                AppliedStrategies = new List<string>()
            };

            foreach (var strategy in strategies)
            {
                result.AppliedStrategies.Add(strategy.MitigationId);
                result.ReductionFactor += strategy.ReductionFactor;
            }

            result.FinalRisk = result.InitialRisk * (1 - result.ReductionFactor);
            _logger.LogInformation($"Ground Risk Mitigation for {operation.OperationId}");

            return result;
        }

        public RiskMitigationResult MitigateAirRisk(Operation operation)
        {
            var strategies = RiskMitigation.GetDefaultStrategies(operation.SoraVersion)
                .Where(s => s.Type == RiskMitigationType.AirRisk)
                .ToList();

            var result = new RiskMitigationResult
            {
                InitialRisk = CalculateInitialAirRisk(operation),
                SoraVersion = operation.SoraVersion,
                AppliedStrategies = new List<string>()
            };

            foreach (var strategy in strategies)
            {
                result.AppliedStrategies.Add(strategy.MitigationId);
                result.ReductionFactor += strategy.ReductionFactor;
            }

            result.FinalRisk = result.InitialRisk * (1 - result.ReductionFactor);
            _logger.LogInformation($"Air Risk Mitigation for {operation.OperationId}");

            return result;
        }

        public int ApplyFloorRule(Operation operation)
        {
            var arcOrder = operation.SoraVersion == "2.0" 
                ? new[] { "ARC-d", "ARC-c", "ARC-b", "ARC-a" }
                : new[] { "ARC-d", "ARC-c", "ARC-b", "ARC-a" };

            var currentArcIndex = Array.IndexOf(arcOrder, operation.ARC);
            
            // M3 penalties reduce ARC
            var m3Penalties = operation.OSORequirements
                .Count(r => r.M3PenaltyApplies);

            var newArcIndex = Math.Min(currentArcIndex + m3Penalties, arcOrder.Length - 1);
            
            _logger.LogInformation(
                $"Floor Rule: ARC changed from {operation.ARC} to {arcOrder[newArcIndex]}"
            );

            return newArcIndex;
        }

        public int RecalculateSAIL(Operation operation)
        {
            var groundRiskResult = MitigateGroundRisk(operation);
            var airRiskResult = MitigateAirRisk(operation);

            // SAIL calculation differs between SORA versions
            var sailAdjustment = operation.SoraVersion switch
            {
                "2.0" => CalculateSailSora20(groundRiskResult, airRiskResult),
                "2.5" => CalculateSailSora25(groundRiskResult, airRiskResult),
                _ => throw new ArgumentException($"Invalid SORA version: {operation.SoraVersion}")
            };

            _logger.LogInformation(
                $"SAIL Recalculation: {operation.SAIL} → {sailAdjustment}"
            );

            return sailAdjustment;
        }

        private double CalculateInitialGroundRisk(Operation operation)
        {
            // Simplified ground risk calculation
            return operation.SoraVersion == "2.0" ? 0.7 : 0.6;
        }

        private double CalculateInitialAirRisk(Operation operation)
        {
            // Simplified air risk calculation
            return operation.SoraVersion == "2.0" ? 0.6 : 0.5;
        }

        private int CalculateSailSora20(
            RiskMitigationResult groundRisk, 
            RiskMitigationResult airRisk)
        {
            // SORA 2.0 SAIL calculation logic
            var reduction = (groundRisk.ReductionFactor + airRisk.ReductionFactor) / 2;
            return Math.Max(0, (int)Math.Round(reduction * 6));
        }

        private int CalculateSailSora25(
            RiskMitigationResult groundRisk, 
            RiskMitigationResult airRisk)
        {
            // SORA 2.5 SAIL calculation logic
            var reduction = (groundRisk.ReductionFactor + airRisk.ReductionFactor) / 2;
            return Math.Max(0, (int)Math.Round(reduction * 5));
        }
    }
}