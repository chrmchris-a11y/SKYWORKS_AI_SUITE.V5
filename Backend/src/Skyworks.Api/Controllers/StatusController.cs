using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Skyworks.Api.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public StatusController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                // ContentRootPath -> Backend/src/Skyworks.Api
                // We need repoRoot/Docs/Knowledge/PROJECT_STATUS.json
                var statusPath = Path.GetFullPath(
                    Path.Combine(_env.ContentRootPath, "..", "..", "..", "Docs", "Knowledge", "PROJECT_STATUS.json")
                );

                if (!System.IO.File.Exists(statusPath))
                {
                    return NotFound(new { error = "STATUS_NOT_FOUND", path = statusPath });
                }

                var json = System.IO.File.ReadAllText(statusPath);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, title: "Failed to read project status");
            }
        }
    }
}
