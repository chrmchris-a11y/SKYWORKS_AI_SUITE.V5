using System.Threading.Tasks;

namespace SORA.Compliance.Services
{
    /// <summary>
    /// Service interface for SAIL calculations and OSO requirements.
    /// Provides clean interface for clients with parity to Python implementation.
    /// 
    /// References:
    /// - SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947
    /// - SORA 2.5: JARUS SORA v2.5 Main Body & Annexes
    /// </summary>
    public interface ISAILService
    {
        /// <summary>
        /// Calculate SAIL for SORA 2.0 using GRC and letter ARC.
        /// Reference: EASA AMC/GM to Reg. (EU) 2019/947, Annex D, Table D.1
        /// </summary>
        /// <param name="grc">Ground Risk Class (1-7, >7 returns Category C)</param>
        /// <param name="arcLetter">ARC letter (a, b, c, d)</param>
        /// <returns>SAIL calculation result</returns>
        Task<SAILResult> CalculateSail20Async(int grc, char arcLetter);

        /// <summary>
        /// Calculate SAIL for SORA 2.5 using GRC and numeric residual ARC.
        /// Reference: JARUS SORA v2.5 Main Body & Annexes, Step 9 / Annex D, Table 7
        /// </summary>
        /// <param name="grc">Ground Risk Class (1-10)</param>
        /// <param name="residualArcLevel">Numeric residual ARC level (1-10)</param>
        /// <returns>SAIL calculation result</returns>
        Task<SAILResult> CalculateSail25Async(int grc, int residualArcLevel);

        /// <summary>
        /// Get OSO requirements for a specific SAIL and SORA version.
        /// </summary>
        /// <param name="sail">SAIL level (I, II, III, IV, V, VI)</param>
        /// <param name="soraVersion">SORA version (2.0 or 2.5)</param>
        /// <returns>OSO requirements with robustness information</returns>
        Task<OSOResponse> GetOSORequirementsAsync(string sail, string soraVersion);

        /// <summary>
        /// Get the complete SAIL mapping table for UI display.
        /// </summary>
        /// <param name="soraVersion">SORA version (2.0 or 2.5)</param>
        /// <returns>Complete mapping table data</returns>
        Task<MappingTable> GetMappingTableAsync(string soraVersion);
    }
}

