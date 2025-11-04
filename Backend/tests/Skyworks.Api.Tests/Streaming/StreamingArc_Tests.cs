using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Skyworks.Core.Models.Airspace;
using Skyworks.Core.Models.Streaming;
using Skyworks.Core.Models.ARC;
using Skyworks.Core.Services.Streaming;
using Skyworks.Core.Services.Airspace;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services.ARC;
using Skyworks.Core.Models.ARC.Validation;
using System.Collections.Generic;

namespace Skyworks.Api.Tests.Streaming
{
    public class StreamingArc_Tests
    {
        private class FakeAirspaceProvider : IAirspaceProvider
        {
            public int Calls { get; private set; } = 0;
            public Task<AirspaceQueryResult> GetAirspacesAtPointAsync(GeoPoint point, CancellationToken ct = default)
            {
                Calls++;
                return Task.FromResult(new AirspaceQueryResult
                {
                    Airspaces = new List<AirspaceInfo> { new AirspaceInfo { Id = "X", Name = "CTR", Type = AirspaceType.CTR } }
                });
            }

            public Task<List<AirspaceInfo>> GetAirspacesIntersectingPolygonAsync(IEnumerable<GeoPoint> polygon, double? minAltitudeMeters = null, double? maxAltitudeMeters = null, CancellationToken ct = default)
                => Task.FromResult(new List<AirspaceInfo>());
        }

        [Fact]
        public async Task Streaming_Produces_Event_And_Caches_Airspace()
        {
            var air = new FakeAirspaceProvider();
            var svc = new StreamingArcService(new GRCCalculationService(), new ARCValidationService(), air, zones: null, debounce: null, airspaceTtl: System.TimeSpan.FromSeconds(5));

            var update = new TelemetryUpdate
            {
                DroneId = "d1",
                SessionId = "s1",
                Position = new GeoPoint { Latitude = 34.9, Longitude = 33.62, AltitudeMeters = 60 }
            };

            var ev1 = await svc.ProcessTelemetry_V2_5Async(update);
            var ev2 = await svc.ProcessTelemetry_V2_5Async(update);

            Assert.Equal("d1", ev1.DroneId);
            Assert.Equal(1, air.Calls); // δεύτερη κλήση πρέπει να εξυπηρετηθεί από cache
            Assert.True(ev1.AirspacesCount >= 1);
            Assert.InRange((int)ev1.InitialARC, 0, 3);
        }
    }
}
