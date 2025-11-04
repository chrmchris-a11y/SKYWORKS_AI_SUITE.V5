using Skyworks.Core.Models.ARC.Composite;

namespace Skyworks.Core.Services.ARC;

public interface IARCCompositeService
{
    CompositeArcResult ComputeInitial_V2_5(CompositeArcRequest request);
}
