using Microsoft.Extensions.Logging;
using Skyworks.Backend.Models;

namespace Skyworks.Backend.Services
{
    /// <summary>
    /// Υπηρεσία για επικύρωση OSO (Operational Safety Objectives)
    /// Βάσει JARUS SORA 2.5 Annex B - OSO #1 έως #24
    /// Υλοποιεί σύνθετους αλγόριθμους για OSO #11, #17, #19, #23
    /// </summary>
    public class OSOService : IOSOService
    {
        private readonly ILogger<OSOService> _logger;
        private readonly Dictionary<string, OSORequirement> _osoRequirements;

        public OSOService(ILogger<OSOService> logger)
        {
            _logger = logger;
            _osoRequirements = InitializeOSORequirements();
        }

        #region OSO #11 - Detect & Avoid (Strategic/Tactical)

        /// <summary>
        /// Επικύρωση OSO #11: Detect & Avoid
        /// Strategic για ARC-a/b: Pre-flight planning + NOTAMs
        /// Tactical για ARC-c/d: Ενεργό σύστημα DnA ή visual observers
        /// </summary>
        /// <param name="operation">Στοιχεία operation</param>
        /// <param name="arc">ARC level (ARC-a έως ARC-d)</param>
        /// <returns>Αποτέλεσμα OSO validation</returns>
        public OSOResult ValidateDetectAndAvoid(Operation operation, string arc)
        {
            _logger.LogInformation("Validating OSO #11 (Detect & Avoid) for ARC: {ARC}", arc);

            var result = new OSOResult
            {
                OSOId = "OSO-11",
                IsCompliant = false,
                Confidence = 0.0,
                RequiredEvidence = new List<string>(),
                M3PenaltyApplied = false,
                ValidationMessages = new List<string>()
            };

            try
            {
                // Strategic mitigation για ARC-a/ARC-b
                if (arc == "ARC-a" || arc == "ARC-b")
                {
                    var strategicResult = ValidateStrategicDetectAndAvoid(operation);
                    result.IsCompliant = strategicResult.IsCompliant;
                    result.Confidence = strategicResult.Confidence;
                    result.RequiredEvidence.AddRange(strategicResult.RequiredEvidence);
                    result.ValidationMessages.AddRange(strategicResult.ValidationMessages);

                    if (result.IsCompliant)
                    {
                        result.Details = "Strategic Detect & Avoid requirements satisfied for low air risk.";
                    }
                }
                // Tactical mitigation για ARC-c/ARC-d
                else if (arc == "ARC-c" || arc == "ARC-d")
                {
                    var tacticalResult = ValidateTacticalDetectAndAvoid(operation);
                    result.IsCompliant = tacticalResult.IsCompliant;
                    result.Confidence = tacticalResult.Confidence;
                    result.RequiredEvidence.AddRange(tacticalResult.RequiredEvidence);
                    result.ValidationMessages.AddRange(tacticalResult.ValidationMessages);

                    // Αν δεν υπάρχει tactical system, εφαρμόζουμε M3 penalty
                    if (!result.IsCompliant)
                    {
                        var tmprAvailable = CheckTMPRForOSO(operation, "OSO-11");
                        if (!tmprAvailable)
                        {
                            result.M3PenaltyApplied = true;
                            result.ValidationMessages.Add("M3 Penalty applied: No tactical DnA system or TMPR available");
                            _logger.LogWarning("M3 Penalty applied for OSO #11 - No tactical mitigation");
                        }
                    }

                    result.Details = result.IsCompliant 
                        ? "Tactical Detect & Avoid requirements satisfied for high air risk."
                        : "Tactical Detect & Avoid mitigation required but not available.";
                }
                else
                {
                    result.ValidationMessages.Add($"Invalid ARC level: {arc}");
                    result.Details = "Unknown ARC level provided.";
                }

                _logger.LogInformation("OSO #11 validation completed. Compliant: {IsCompliant}, M3 Applied: {M3Applied}", 
                    result.IsCompliant, result.M3PenaltyApplied);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #11");
                result.ValidationMessages.Add($"Validation error: {ex.Message}");
                result.Details = "Error during OSO #11 validation.";
            }

            return result;
        }

        private OSOResult ValidateStrategicDetectAndAvoid(Operation operation)
        {
            var result = new OSOResult { OSOId = "OSO-11-Strategic" };
            var confidenceScore = 0.0;
            var evidenceCount = 0;

            // Έλεγχος pre-flight airspace analysis
            if (operation.AirRisk.NOTAMs.Any())
            {
                confidenceScore += 0.4;
                evidenceCount++;
                result.ValidationMessages.Add("Pre-flight NOTAMs analysis completed");
            }
            else
            {
                result.RequiredEvidence.Add("NOTAM analysis and airspace restrictions check");
            }

            // Έλεγχος ATC coordination
            if (operation.AirRisk.HasATCService)
            {
                confidenceScore += 0.3;
                evidenceCount++;
                result.ValidationMessages.Add("ATC coordination available");
            }

            // Έλεγχος flight planning procedures
            if (operation.FlightPath.Waypoints.Count > 1)
            {
                confidenceScore += 0.3;
                evidenceCount++;
                result.ValidationMessages.Add("Detailed flight plan with waypoints");
            }
            else
            {
                result.RequiredEvidence.Add("Detailed flight plan with predefined waypoints");
            }

            result.IsCompliant = evidenceCount >= 2; // Τουλάχιστον 2/3 απαιτήσεις
            result.Confidence = confidenceScore;

            if (!result.IsCompliant)
            {
                result.RequiredEvidence.Add("Strategic airspace analysis documentation");
            }

            return result;
        }

        private OSOResult ValidateTacticalDetectAndAvoid(Operation operation)
        {
            var result = new OSOResult { OSOId = "OSO-11-Tactical" };
            var confidenceScore = 0.0;

            // Έλεγχος για ενεργό DnA σύστημα
            var hasTacticalSystem = operation.TMPRSystems.Any(t => 
                t.SystemType.Contains("detect") && t.IsActiveForOSO);

            if (hasTacticalSystem)
            {
                confidenceScore += 0.7;
                result.IsCompliant = true;
                result.ValidationMessages.Add("Active tactical Detect & Avoid system detected");
            }
            else
            {
                // Έλεγχος για visual observers (alternative)
                var hasVisualObservers = operation.Crew.GroundCrew.Count >= 2 &&
                    operation.Crew.GroundCrew.All(c => c.TrainingValid);

                if (hasVisualObservers)
                {
                    confidenceScore += 0.5;
                    result.IsCompliant = true;
                    result.ValidationMessages.Add("Sufficient trained visual observers available");
                }
                else
                {
                    result.RequiredEvidence.Add("Tactical DnA system OR ≥2 trained visual observers");
                    result.ValidationMessages.Add("High air risk requires tactical mitigation");
                }
            }

            result.Confidence = confidenceScore;
            return result;
        }

        #endregion

        #region OSO #17 - Operational Volume Definition

        /// <summary>
        /// Επικύρωση OSO #17: Ορισμός Operational Volume
        /// 3D όγκος (lat/lon/altitude + buffer) με wind drift, GPS errors
        /// Διαχωρισμός από manned traffic
        /// </summary>
        /// <param name="operation">Στοιχεία operation</param>
        /// <returns>Αποτέλεσμα OSO validation</returns>
        public OSOResult ValidateOperationalVolume(Operation operation)
        {
            _logger.LogInformation("Validating OSO #17 (Operational Volume Definition)");

            var result = new OSOResult
            {
                OSOId = "OSO-17",
                IsCompliant = false,
                Confidence = 0.0,
                RequiredEvidence = new List<string>(),
                ValidationMessages = new List<string>()
            };

            try
            {
                var confidenceScore = 0.0;
                var validationIssues = 0;

                // 1. Έλεγχος ορισμού 3D volume
                var volumeValid = Validate3DVolume(operation.OperationalVolume);
                if (volumeValid.IsValid)
                {
                    confidenceScore += 0.3;
                    result.ValidationMessages.Add("3D operational volume properly defined");
                }
                else
                {
                    validationIssues++;
                    result.RequiredEvidence.Add("Complete 3D operational volume definition (lat/lon/altitude)");
                    result.ValidationMessages.AddRange(volumeValid.Issues);
                }

                // 2. Έλεγχος safety buffer
                var bufferValid = ValidateSafetyBuffer(operation);
                if (bufferValid.IsValid)
                {
                    confidenceScore += 0.25;
                    result.ValidationMessages.Add($"Safety buffer adequate: {operation.OperationalVolume.BufferRadius}m");
                }
                else
                {
                    validationIssues++;
                    result.RequiredEvidence.Add("Safety buffer for GPS accuracy & wind drift");
                    result.ValidationMessages.AddRange(bufferValid.Issues);
                }

                // 3. Έλεγχος segregation από manned traffic
                var segregationValid = ValidateTrafficSegregation(operation);
                if (segregationValid.IsValid)
                {
                    confidenceScore += 0.25;
                    result.ValidationMessages.Add("Segregation from manned traffic validated");
                }
                else
                {
                    validationIssues++;
                    result.RequiredEvidence.Add("Proof of segregation from manned aircraft");
                    result.ValidationMessages.AddRange(segregationValid.Issues);
                }

                // 4. Έλεγχος controlled airspace overlap
                var airspaceValid = ValidateControlledAirspaceCompliance(operation);
                if (airspaceValid.IsValid)
                {
                    confidenceScore += 0.2;
                    result.ValidationMessages.Add("Controlled airspace compliance verified");
                }
                else
                {
                    validationIssues++;
                    result.RequiredEvidence.Add("ATC coordination for controlled airspace operations");
                    result.ValidationMessages.AddRange(airspaceValid.Issues);
                }

                result.IsCompliant = validationIssues == 0;
                result.Confidence = confidenceScore;

                if (result.IsCompliant)
                {
                    result.Details = "Operational volume properly defined with adequate safety margins.";
                }
                else
                {
                    result.Details = $"Operational volume definition has {validationIssues} validation issues.";
                    
                    // Check for TMPR if not compliant
                    var tmprAvailable = CheckTMPRForOSO(operation, "OSO-17");
                    if (!tmprAvailable)
                    {
                        result.M3PenaltyApplied = true;
                        result.ValidationMessages.Add("M3 Penalty applied: Inadequate operational volume definition");
                    }
                }

                _logger.LogInformation("OSO #17 validation completed. Compliant: {IsCompliant}, Issues: {Issues}", 
                    result.IsCompliant, validationIssues);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #17");
                result.ValidationMessages.Add($"Validation error: {ex.Message}");
                result.Details = "Error during OSO #17 validation.";
            }

            return result;
        }

        private (bool IsValid, List<string> Issues) Validate3DVolume(OperationalVolume volume)
        {
            var issues = new List<string>();
            var isValid = true;

            if (volume.LatitudeMin >= volume.LatitudeMax)
            {
                issues.Add("Invalid latitude bounds");
                isValid = false;
            }

            if (volume.LongitudeMin >= volume.LongitudeMax)
            {
                issues.Add("Invalid longitude bounds");
                isValid = false;
            }

            if (volume.AltitudeMin >= volume.AltitudeMax)
            {
                issues.Add("Invalid altitude bounds");
                isValid = false;
            }

            if (volume.AltitudeMax > 120) // EASA standard limit
            {
                issues.Add("Altitude exceeds 120m AGL limit without authorization");
            }

            return (isValid, issues);
        }

        private (bool IsValid, List<string> Issues) ValidateSafetyBuffer(Operation operation)
        {
            var issues = new List<string>();
            var requiredBuffer = CalculateMinimumBuffer(operation);
            var actualBuffer = operation.OperationalVolume.BufferRadius;

            if (actualBuffer < requiredBuffer)
            {
                issues.Add($"Buffer too small: {actualBuffer}m (required: {requiredBuffer}m)");
                return (false, issues);
            }

            return (true, issues);
        }

        private double CalculateMinimumBuffer(Operation operation)
        {
            // Βασικό buffer: 10% του συνολικού μήκους
            var baseBuffer = operation.FlightPath.TotalDistance * 0.1;
            
            // GPS accuracy buffer: 3m σε horizontal, 5m σε vertical
            var gpsBuffer = Math.Max(3.0, 5.0);
            
            // Wind drift buffer: βάσει διάρκειας και μέγιστης ταχύτητας ανέμου (υποθέτουμε 10 m/s)
            var windBuffer = (operation.FlightDurationMinutes / 60.0) * 10.0;

            return Math.Max(baseBuffer, Math.Max(gpsBuffer, windBuffer));
        }

        private (bool IsValid, List<string> Issues) ValidateTrafficSegregation(Operation operation)
        {
            var issues = new List<string>();
            
            // Έλεγχος για airports εντός 5 NM
            var nearAirports = CheckNearbyAirports(operation.OperationalVolume);
            if (nearAirports.Any())
            {
                issues.Add($"Operation within 5 NM of airports: {string.Join(", ", nearAirports)}");
                return (false, issues);
            }

            // Έλεγχος air traffic density
            if (operation.AirRisk.AirTrafficDensity > 0.5) // High density
            {
                if (!operation.AirRisk.HasATCService)
                {
                    issues.Add("High traffic density requires ATC coordination");
                    return (false, issues);
                }
            }

            return (true, issues);
        }

        private List<string> CheckNearbyAirports(OperationalVolume volume)
        {
            // Simplified check - σε πραγματική υλοποίηση θα χρησιμοποιούσαμε GIS database
            var nearbyAirports = new List<string>();
            
            // Placeholder logic - θα αντικατασταθεί με πραγματικά δεδομένα
            if (volume.LatitudeMin > 37.9 && volume.LatitudeMax < 38.0 &&
                volume.LongitudeMin > 23.7 && volume.LongitudeMax < 23.8)
            {
                nearbyAirports.Add("LGAV (Athens Eleftherios Venizelos)");
            }

            return nearbyAirports;
        }

        private (bool IsValid, List<string> Issues) ValidateControlledAirspaceCompliance(Operation operation)
        {
            var issues = new List<string>();

            if (operation.AirRisk.AirspaceClass == "C" || operation.AirRisk.AirspaceClass == "D")
            {
                if (!operation.AirRisk.HasATCService)
                {
                    issues.Add($"Class {operation.AirRisk.AirspaceClass} airspace requires ATC coordination");
                    return (false, issues);
                }
            }

            return (true, issues);
        }

        #endregion

        #region OSO #19 - Human Performance (Crew Training)

        /// <summary>
        /// Επικύρωση OSO #19: Human Performance
        /// Remote pilot certification, ground crew training, training records
        /// </summary>
        /// <param name="operation">Στοιχεία operation</param>
        /// <returns>Αποτέλεσμα OSO validation</returns>
        public OSOResult ValidateHumanPerformance(Operation operation)
        {
            _logger.LogInformation("Validating OSO #19 (Human Performance)");

            var result = new OSOResult
            {
                OSOId = "OSO-19",
                IsCompliant = false,
                Confidence = 0.0,
                RequiredEvidence = new List<string>(),
                ValidationMessages = new List<string>()
            };

            try
            {
                var crewScore = CalculateCrewScore(operation.Crew);
                result.Confidence = crewScore.ConfidenceLevel;

                // Minimum crew score for compliance = 4 (από το compact summary)
                if (crewScore.TotalScore >= 4)
                {
                    result.IsCompliant = true;
                    result.ValidationMessages.Add($"Crew competency adequate (Score: {crewScore.TotalScore}/5)");
                    result.Details = "Human performance requirements satisfied.";
                }
                else
                {
                    result.IsCompliant = false;
                    result.ValidationMessages.Add($"Crew competency insufficient (Score: {crewScore.TotalScore}/5, Required: 4)");
                    result.RequiredEvidence.Add("Additional crew training or certification");
                    result.Details = $"Crew score {crewScore.TotalScore} below minimum requirement of 4.";

                    // Check για TMPR ή additional training
                    var tmprAvailable = CheckTMPRForOSO(operation, "OSO-19");
                    if (!tmprAvailable)
                    {
                        result.M3PenaltyApplied = true;
                        result.ValidationMessages.Add("M3 Penalty applied: Insufficient crew competency without mitigation");
                    }
                }

                result.ValidationMessages.AddRange(crewScore.Details);
                
                if (crewScore.RequiredEvidence.Any())
                {
                    result.RequiredEvidence.AddRange(crewScore.RequiredEvidence);
                }

                _logger.LogInformation("OSO #19 validation completed. Score: {Score}/5, Compliant: {IsCompliant}", 
                    crewScore.TotalScore, result.IsCompliant);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #19");
                result.ValidationMessages.Add($"Validation error: {ex.Message}");
                result.Details = "Error during OSO #19 validation.";
            }

            return result;
        }

        private (int TotalScore, double ConfidenceLevel, List<string> Details, List<string> RequiredEvidence) 
            CalculateCrewScore(CrewData crew)
        {
            var totalScore = 0;
            var details = new List<string>();
            var evidence = new List<string>();
            var maxScore = 5;

            // Remote pilot assessment (max 3 points)
            var pilot = crew.RemotePilot;
            
            if (pilot.Certification == "EASA_A1_A3" || pilot.Certification.Contains("EASA"))
            {
                totalScore += 3;
                details.Add($"Remote pilot has valid EASA certification: {pilot.Certification}");
            }
            else if (pilot.FlightHours >= 50)
            {
                totalScore += 2;
                details.Add($"Remote pilot has sufficient experience: {pilot.FlightHours} hours");
            }
            else if (pilot.FlightHours > 0)
            {
                totalScore += 1;
                details.Add($"Remote pilot has limited experience: {pilot.FlightHours} hours");
            }
            else
            {
                details.Add("Remote pilot certification/experience insufficient");
                evidence.Add("Remote pilot EASA certification or minimum 50 flight hours");
            }

            // Training validity check
            if (!pilot.IsTrainingValid)
            {
                totalScore = Math.Max(0, totalScore - 1);
                details.Add($"Remote pilot training expired: {pilot.LastTrainingDate:yyyy-MM-dd}");
                evidence.Add("Current training certification (within 12 months)");
            }

            // Ground crew assessment (max 2 points)
            var trainedGroundCrew = crew.GroundCrew.Count(c => c.TrainingValid);
            if (trainedGroundCrew >= 2)
            {
                totalScore += 2;
                details.Add($"Ground crew adequately trained: {trainedGroundCrew} members");
            }
            else if (trainedGroundCrew >= 1)
            {
                totalScore += 1;
                details.Add($"Minimal ground crew training: {trainedGroundCrew} member(s)");
            }
            else
            {
                details.Add("Ground crew training insufficient");
                evidence.Add("Training records for ground crew emergency procedures");
            }

            var confidenceLevel = (double)totalScore / maxScore;
            
            return (totalScore, confidenceLevel, details, evidence);
        }

        #endregion

        #region OSO #23 - Adjacent Area (Population Density)

        /// <summary>
        /// Επικύρωση OSO #23: Adjacent Area Analysis
        /// Περιοχές εντός 1 NM από flight path, population density classification
        /// CGR vs Uncontrolled areas, mitigation requirements
        /// </summary>
        /// <param name="operation">Στοιχεία operation</param>
        /// <param name="grc">Ground Risk Class</param>
        /// <returns>Αποτέλεσμα OSO validation</returns>
        public OSOResult ValidateAdjacentArea(Operation operation, int grc)
        {
            _logger.LogInformation("Validating OSO #23 (Adjacent Area Analysis) with GRC: {GRC}", grc);

            var result = new OSOResult
            {
                OSOId = "OSO-23",
                IsCompliant = false,
                Confidence = 0.0,
                RequiredEvidence = new List<string>(),
                ValidationMessages = new List<string>()
            };

            try
            {
                var adjacentAreas = AnalyzeAdjacentAreas(operation);
                var riskAssessment = AssessAdjacentAreaRisks(adjacentAreas, operation.GroundRisk);
                
                result.ValidationMessages.Add($"Analyzed {adjacentAreas.Count} adjacent areas within 1 NM");
                
                var confidenceScore = 0.0;
                var mitigationRequired = false;

                foreach (var area in adjacentAreas)
                {
                    if (area.AreaType == "UNCONTROLLED" && area.PopulationDensity > 100) // per km²
                    {
                        mitigationRequired = true;
                        
                        // Έλεγχος για strategic mitigation (flight altitude > 150m AGL)
                        if (operation.FlightPath.MaxAltitudeAGL > 150)
                        {
                            confidenceScore += 0.3;
                            result.ValidationMessages.Add($"High-density uncontrolled area mitigated by altitude: {operation.FlightPath.MaxAltitudeAGL}m AGL");
                        }
                        else
                        {
                            result.ValidationMessages.Add($"High-density area requires mitigation: {area.AreaName} ({area.PopulationDensity}/km²)");
                            result.RequiredEvidence.Add($"Strategic mitigation for high-density area: {area.AreaName}");
                        }
                    }
                    else if (area.AreaType == "CONTROLLED")
                    {
                        confidenceScore += 0.2;
                        result.ValidationMessages.Add($"Controlled ground area identified: {area.AreaName}");
                    }
                }

                // GRC impact assessment
                var grcImpact = CalculateGRCImpact(adjacentAreas);
                if (grcImpact > 0)
                {
                    result.ValidationMessages.Add($"Adjacent areas impact GRC by +{grcImpact}");
                    result.Details = $"High-density adjacent areas increase Ground Risk Class by {grcImpact}.";
                }

                // Overall compliance assessment
                if (!mitigationRequired || confidenceScore > 0.5)
                {
                    result.IsCompliant = true;
                    result.Confidence = Math.Min(1.0, 0.5 + confidenceScore);
                    result.Details = result.Details ?? "Adjacent area risks adequately managed.";
                }
                else
                {
                    result.IsCompliant = false;
                    result.Confidence = confidenceScore;
                    result.Details = result.Details ?? "Adjacent area mitigation required but not implemented.";
                    
                    // Check for TMPR
                    var tmprAvailable = CheckTMPRForOSO(operation, "OSO-23");
                    if (!tmprAvailable)
                    {
                        result.M3PenaltyApplied = true;
                        result.ValidationMessages.Add("M3 Penalty applied: High-density adjacent areas without adequate mitigation");
                    }
                }

                _logger.LogInformation("OSO #23 validation completed. Adjacent areas: {Count}, Mitigation required: {Required}, Compliant: {IsCompliant}", 
                    adjacentAreas.Count, mitigationRequired, result.IsCompliant);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OSO #23");
                result.ValidationMessages.Add($"Validation error: {ex.Message}");
                result.Details = "Error during OSO #23 validation.";
            }

            return result;
        }

        private List<AdjacentArea> AnalyzeAdjacentAreas(Operation operation)
        {
            var areas = new List<AdjacentArea>();
            const double radiusNM = 1.0; // 1 nautical mile
            const double radiusKm = radiusNM * 1.852; // Convert to km

            // Simplified analysis - σε πραγματική υλοποίηση θα χρησιμοποιούσαμε GIS
            foreach (var waypoint in operation.FlightPath.Waypoints)
            {
                var area = new AdjacentArea
                {
                    AreaName = $"Area_{waypoint.Latitude:F3}_{waypoint.Longitude:F3}",
                    CenterLat = waypoint.Latitude,
                    CenterLon = waypoint.Longitude,
                    RadiusKm = radiusKm,
                    PopulationDensity = EstimatePopulationDensity(waypoint.Latitude, waypoint.Longitude),
                    AreaType = DetermineAreaType(waypoint.Latitude, waypoint.Longitude)
                };
                
                areas.Add(area);
            }

            return areas;
        }

        private double EstimatePopulationDensity(double lat, double lon)
        {
            // Simplified estimation - σε πραγματική εφαρμογή θα χρησιμοποιούσαμε demographic database
            // Athens area check (high density)
            if (lat >= 37.8 && lat <= 38.1 && lon >= 23.6 && lon <= 23.9)
            {
                return 400.0; // High density urban
            }
            // Thessaloniki area check
            else if (lat >= 40.5 && lat <= 40.7 && lon >= 22.8 && lon <= 23.1)
            {
                return 200.0; // Medium density urban
            }
            else
            {
                return 50.0; // Rural/sparsely populated
            }
        }

        private string DetermineAreaType(double lat, double lon)
        {
            // Simplified determination - πραγματικά δεδομένα θα έρχονταν από regulatory database
            if (EstimatePopulationDensity(lat, lon) > 150)
            {
                return "UNCONTROLLED"; // Urban areas typically uncontrolled ground
            }
            else
            {
                return "CONTROLLED"; // Rural areas more likely to be controlled
            }
        }

        private int CalculateGRCImpact(List<AdjacentArea> areas)
        {
            var highDensityAreas = areas.Count(a => a.AreaType == "UNCONTROLLED" && a.PopulationDensity > 150);
            
            if (highDensityAreas >= 3)
                return 2; // Significant impact
            else if (highDensityAreas >= 1)
                return 1; // Moderate impact
            else
                return 0; // No impact
        }

        private (bool HasRisks, List<string> Mitigations) AssessAdjacentAreaRisks(List<AdjacentArea> areas, GroundRisk groundRisk)
        {
            var hasRisks = false;
            var mitigations = new List<string>();

            foreach (var area in areas)
            {
                if (area.PopulationDensity > 200 && area.AreaType == "UNCONTROLLED")
                {
                    hasRisks = true;
                    mitigations.Add($"High altitude operation (>150m AGL) over {area.AreaName}");
                    mitigations.Add($"Time-of-day restrictions for {area.AreaName}");
                }
            }

            return (hasRisks, mitigations);
        }

        #endregion

        #region M3 Penalty & TMPR Logic

        /// <summary>
        /// Εφαρμογή M3 penalty για OSO non-compliance
        /// Μειώνει το SAIL κατά 1 level (minimum SAIL = 0)
        /// </summary>
        /// <param name="initialSAIL">Αρχικό SAIL level</param>
        /// <param name="tmpr">TMPR system (null αν δεν υπάρχει)</param>
        /// <returns>Final SAIL after M3 penalty</returns>
        private int ApplyM3Penalty(int initialSAIL, TMPRSystem? tmpr)
        {
            if (tmpr != null && tmpr.ReliabilityFactor >= 0.9) // High reliability TMPR
            {
                _logger.LogInformation("M3 Penalty avoided - TMPR system available with reliability: {Reliability}", 
                    tmpr.ReliabilityFactor);
                return initialSAIL;
            }

            var finalSAIL = Math.Max(0, initialSAIL - 1);
            _logger.LogWarning("M3 Penalty applied: SAIL reduced from {Initial} to {Final}", initialSAIL, finalSAIL);
            
            return finalSAIL;
        }

        /// <summary>
        /// Έλεγχος για διαθέσιμο TMPR system για συγκεκριμένο OSO
        /// </summary>
        /// <param name="operation">Operation data</param>
        /// <param name="osoId">OSO identifier</param>
        /// <returns>True αν υπάρχει κατάλληλο TMPR</returns>
        private bool CheckTMPRForOSO(Operation operation, string osoId)
        {
            return operation.TMPRSystems.Any(t => 
                t.ApplicableOSO == osoId && 
                t.IsActiveForOSO && 
                t.ReliabilityFactor >= 0.9);
        }

        #endregion

        #region OSO Requirements Initialization

        private Dictionary<string, OSORequirement> InitializeOSORequirements()
        {
            return new Dictionary<string, OSORequirement>
            {
                {
                    "OSO-11",
                    new OSORequirement
                    {
                        OSOId = "OSO-11",
                        Description = "Detect and Avoid (Strategic/Tactical)",
                        Category = "Air Risk Mitigation",
                        MinimumSAIL = 2,
                        MaximumSAIL = 6,
                        HasTMPR = true,
                        Weight = 5
                    }
                },
                {
                    "OSO-17",
                    new OSORequirement
                    {
                        OSOId = "OSO-17",
                        Description = "Operational Volume Definition",
                        Category = "Strategic Mitigation",
                        MinimumSAIL = 1,
                        MaximumSAIL = 6,
                        HasTMPR = false,
                        Weight = 3
                    }
                },
                {
                    "OSO-19",
                    new OSORequirement
                    {
                        OSOId = "OSO-19",
                        Description = "Human Performance (Crew Training)",
                        Category = "Human Factors",
                        MinimumSAIL = 1,
                        MaximumSAIL = 6,
                        HasTMPR = false,
                        Weight = 4
                    }
                },
                {
                    "OSO-23",
                    new OSORequirement
                    {
                        OSOId = "OSO-23",
                        Description = "Adjacent Area Analysis",
                        Category = "Ground Risk Mitigation",
                        MinimumSAIL = 2,
                        MaximumSAIL = 6,
                        HasTMPR = true,
                        Weight = 3
                    }
                }
            };
        }

        #endregion

        #region Public Interface Methods

        /// <summary>
        /// Επιστροφή όλων των OSO requirements
        /// </summary>
        public Dictionary<string, OSORequirement> GetAllOSORequirements()
        {
            return _osoRequirements;
        }

        /// <summary>
        /// Επικύρωση όλων των OSO για operation
        /// </summary>
        /// <param name="operation">Operation data</param>
        /// <param name="currentSAIL">Current SAIL level</param>
        /// <returns>Λίστα αποτελεσμάτων OSO validation</returns>
        public List<OSOResult> ValidateAllOSO(Operation operation, int currentSAIL)
        {
            var results = new List<OSOResult>();

            try
            {
                _logger.LogInformation("Validating all OSO requirements for SAIL {SAIL}", currentSAIL);

                // OSO #11
                results.Add(ValidateDetectAndAvoid(operation, operation.ARC));

                // OSO #17  
                results.Add(ValidateOperationalVolume(operation));

                // OSO #19
                results.Add(ValidateHumanPerformance(operation));

                // OSO #23
                results.Add(ValidateAdjacentArea(operation, operation.GRC));

                var compliantCount = results.Count(r => r.IsCompliant);
                var penaltyCount = results.Count(r => r.M3PenaltyApplied);

                _logger.LogInformation("OSO validation completed. {Compliant}/{Total} compliant, {Penalties} penalties applied",
                    compliantCount, results.Count, penaltyCount);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comprehensive OSO validation");
            }

            return results;
        }

        #endregion
    }

    #region Helper Classes & Interfaces

    public interface IOSOService
    {
        OSOResult ValidateDetectAndAvoid(Operation operation, string arc);
        OSOResult ValidateOperationalVolume(Operation operation);
        OSOResult ValidateHumanPerformance(Operation operation);
        OSOResult ValidateAdjacentArea(Operation operation, int grc);
        Dictionary<string, OSORequirement> GetAllOSORequirements();
        List<OSOResult> ValidateAllOSO(Operation operation, int currentSAIL);
    }

    public class AdjacentArea
    {
        public string AreaName { get; set; } = string.Empty;
        public double CenterLat { get; set; }
        public double CenterLon { get; set; }
        public double RadiusKm { get; set; }
        public double PopulationDensity { get; set; } // per km²
        public string AreaType { get; set; } = string.Empty; // CONTROLLED, UNCONTROLLED
    }

    #endregion
}