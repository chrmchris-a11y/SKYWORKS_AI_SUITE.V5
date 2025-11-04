using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Skyworks.Api.Controllers;

[ApiController]
[Route("api/test")]
public class LocalizationTestController : ControllerBase
{
    private readonly IStringLocalizer<Resources.SharedResources> _localizer;

    public LocalizationTestController(IStringLocalizer<Resources.SharedResources> localizer)
    {
        _localizer = localizer;
    }

    [HttpGet("culture")]
    public ActionResult<object> GetCulture()
    {
        var currentCulture = CultureInfo.CurrentCulture.Name;
        var currentUICulture = CultureInfo.CurrentUICulture.Name;
        var message = _localizer["SoraPipelineName"];
        
        return Ok(new
        {
            currentCulture,
            currentUICulture,
            resourceKey = "SoraPipelineName",
            resourceValue = message.Value,
            resourceNotFound = message.ResourceNotFound,
            allKeys = _localizer.GetAllStrings(includeParentCultures: false)
                .Select(s => new { s.Name, s.Value })
                .Take(5)
                .ToArray()
        });
    }
}
