using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Interface for OSO (Operational Safety Objectives) catalog.
/// Provides access to all OSOs and SAIL-based filtering.
/// </summary>
public interface IOsoCatalog
{
    /// <summary>
    /// Gets all OSOs required for a specific SAIL level.
    /// Per SORA 2.5 Annex D, higher SAIL levels require more OSOs.
    /// </summary>
    /// <param name="sail">SAIL level ("I", "II", "III", "IV", "V", "VI").</param>
    /// <returns>List of required OSOs for that SAIL.</returns>
    IReadOnlyList<OsoItem> GetRequiredForSail(string sail);

    /// <summary>
    /// Gets all available OSOs in the catalog.
    /// </summary>
    /// <returns>Complete list of all OSOs.</returns>
    IReadOnlyList<OsoItem> GetAll();

    /// <summary>
    /// Gets a specific OSO by its code (e.g., "OSO#01").
    /// </summary>
    /// <param name="code">OSO code.</param>
    /// <returns>OSO item or null if not found.</returns>
    OsoItem? GetByCode(string code);
}
