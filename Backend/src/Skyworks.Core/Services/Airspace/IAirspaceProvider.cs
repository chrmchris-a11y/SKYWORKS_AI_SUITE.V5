using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Services.Airspace;

/// <summary>
/// Παροχέας εναέριου χώρου: απαντά τι airspaces ισχύουν για σημείο ή περιοχή.
/// Οι υλοποιήσεις μπορούν να είναι στατικές (demo) ή live (AIXM/tiles).
/// </summary>
public interface IAirspaceProvider
{
	/// <summary>
	/// Επιστρέφει τα airspaces που «καλύπτουν» το σημείο (γεωμετρικά και υψομετρικά).
	/// </summary>
	Task<AirspaceQueryResult> GetAirspacesAtPointAsync(GeoPoint point, CancellationToken ct = default);

	/// <summary>
	/// Επιστρέφει τα airspaces που τέμνουν ένα πολύγωνο επιχειρησιακής περιοχής.
	/// Αν δοθούν υψομετρικά όρια, φιλτράρονται ανάλογα.
	/// </summary>
	Task<List<AirspaceInfo>> GetAirspacesIntersectingPolygonAsync(
		IEnumerable<GeoPoint> polygon,
		double? minAltitudeMeters = null,
		double? maxAltitudeMeters = null,
		CancellationToken ct = default);
}
