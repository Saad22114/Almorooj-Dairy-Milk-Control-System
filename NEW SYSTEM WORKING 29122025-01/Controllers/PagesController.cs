using Microsoft.AspNetCore.Mvc;

namespace FarmersApp.Controllers;

public class PagesController : ControllerBase
{
    private IActionResult HtmlFromViews(string fileName)
    {
        // AppContext.BaseDirectory is bin\Release\net8.0 (or bin\Debug\net8.0)
        // We need to go up 3 levels: Release -> bin -> project_root
        var filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Views", fileName);
        filePath = Path.GetFullPath(filePath);

        if (!System.IO.File.Exists(filePath))
            return NotFound($"View file not found: {filePath}");

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
            Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
            Response.Headers["Expires"] = "0"; // Proxies.
            return PhysicalFile(filePath, "text/html; charset=utf-8");
    }

    [HttpGet]
    [Route("")]
    public IActionResult Index() => HtmlFromViews("index.html");

    [HttpGet]
    [Route("settings")]
    public IActionResult Settings() => HtmlFromViews("settings.html");

    [HttpGet]
    [Route("tools")]
    public IActionResult Tools() => HtmlFromViews("tools.html");

    [HttpGet]
    [Route("reports")]
    public IActionResult Reports() => HtmlFromViews("reports.html");

    [HttpGet]
    [Route("farmer_report")]
    public IActionResult FarmerReport() => HtmlFromViews("farmer_report.html");

    [HttpGet]
    [Route("com_settings")]
    public IActionResult ComSettings() => HtmlFromViews("com_settings.html");

    [HttpGet]
    [Route("register")]
    public IActionResult Register() => HtmlFromViews("register.html");

    [HttpGet]
    [Route("farmers")]
    public IActionResult Farmers() => HtmlFromViews("farmers_management_pro.html");

    [HttpGet]
    [Route("farmers-management")]
    public IActionResult FarmersManagement() => HtmlFromViews("farmers_management.html");
}
