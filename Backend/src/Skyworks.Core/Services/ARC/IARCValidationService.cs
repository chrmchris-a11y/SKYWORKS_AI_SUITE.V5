using Skyworks.Core.Models.ARC;
using Skyworks.Core.Models.ARC.Validation;

namespace Skyworks.Core.Services.ARC;

public interface IARCValidationService
{
    ARCValidationResult ValidateEnvironment_V2_5(ARCEnvironmentInput env);
}
