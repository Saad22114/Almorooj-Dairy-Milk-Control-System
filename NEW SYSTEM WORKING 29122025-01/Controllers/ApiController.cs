using FarmersApp.Models;
using FarmersApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace FarmersApp.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly SerialPortService _serialPortService;
    private readonly SettingsService _settingsService;
    private readonly FarmersService _farmersService;
    private readonly MilkEntryService _milkEntryService;

    public ApiController(
        SerialPortService serialPortService,
        SettingsService settingsService,
        FarmersService farmersService,
        MilkEntryService milkEntryService)
    {
        _serialPortService = serialPortService;
        _settingsService = settingsService;
        _farmersService = farmersService;
        _milkEntryService = milkEntryService;
    }

    // ==================== BACKUP ENDPOINTS ====================

    /// <summary>
    /// Download a full backup ZIP (farmers + milk entries + settings + redacted config)
    /// GET /api/backup/all
    /// </summary>
    [HttpGet("backup/all")]
    public IActionResult DownloadFullBackup()
    {
        var timestamp = DateTime.UtcNow;
        var fileStamp = timestamp.ToString("yyyy-MM-dd_HH-mm-ss");

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var farmers = _farmersService.LoadFarmers();
        var milkEntries = _milkEntryService.GetAllMilkEntries();
        var settings = _settingsService.LoadSettings();

        var meta = new
        {
            backupType = "full",
            createdUtc = timestamp,
            farmersCount = farmers?.Count ?? 0,
            milkEntriesCount = milkEntries?.Count ?? 0,
            notes = "This ZIP contains exported data. Connection strings are intentionally not included."
        };

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddZipJson(zip, "meta.json", meta, jsonOptions);
            AddZipJson(zip, "data/farmers.json", farmers, jsonOptions);
            AddZipJson(zip, "data/milk_entries.json", milkEntries, jsonOptions);
            AddZipJson(zip, "data/settings.json", settings, jsonOptions);

            // Add safe copies of appsettings files (redacted)
            AddZipText(zip, "config/appsettings.json", RedactConnectionStrings(TryReadTextFile("appsettings.json")));
            AddZipText(zip, "config/appsettings.Development.json", RedactConnectionStrings(TryReadTextFile("appsettings.Development.json")));

            // Add raw local settings.json if present (this file typically holds COM/settings; OK to include)
            var localSettingsText = TryReadTextFile("settings.json");
            if (!string.IsNullOrWhiteSpace(localSettingsText))
                AddZipText(zip, "config/settings.json", localSettingsText);
        }

        ms.Position = 0;
        var fileName = $"backup_all_{fileStamp}.zip";
        return File(ms.ToArray(), "application/zip", fileName);
    }

    private static void AddZipJson(ZipArchive zip, string path, object? data, JsonSerializerOptions options)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var json = JsonSerializer.Serialize(data, options);
        writer.Write(json);
    }

    private static void AddZipText(ZipArchive zip, string path, string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var entry = zip.CreateEntry(path, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(text);
    }

    private static string? TryReadTextFile(string fileName)
    {
        try
        {
            var basePath = AppContext.BaseDirectory;
            var contentRoot = Directory.GetCurrentDirectory();

            // Prefer content root (where appsettings.json lives)
            var p1 = Path.Combine(contentRoot, fileName);
            if (System.IO.File.Exists(p1)) return System.IO.File.ReadAllText(p1);

            // Fallback: base directory
            var p2 = Path.Combine(basePath, fileName);
            if (System.IO.File.Exists(p2)) return System.IO.File.ReadAllText(p2);
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static string RedactConnectionStrings(string? jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText)) return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                WriteRedacted(doc.RootElement, writer);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch
        {
            // If not valid JSON, return original
            return jsonText;
        }
    }

    private static void WriteRedacted(JsonElement el, Utf8JsonWriter w)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                w.WriteStartObject();
                foreach (var prop in el.EnumerateObject())
                {
                    if (prop.NameEquals("ConnectionStrings"))
                    {
                        w.WritePropertyName(prop.Name);
                        w.WriteStartObject();
                        foreach (var cs in prop.Value.EnumerateObject())
                        {
                            w.WriteString(cs.Name, "***REDACTED***");
                        }
                        w.WriteEndObject();
                    }
                    else
                    {
                        w.WritePropertyName(prop.Name);
                        WriteRedacted(prop.Value, w);
                    }
                }
                w.WriteEndObject();
                break;
            case JsonValueKind.Array:
                w.WriteStartArray();
                foreach (var item in el.EnumerateArray())
                    WriteRedacted(item, w);
                w.WriteEndArray();
                break;
            default:
                el.WriteTo(w);
                break;
        }
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromBody] ConnectRequest request)
    {
        if (_serialPortService.IsConnected)
            return BadRequest(new { success = false, message = "Already connected" });

        var settings = _settingsService.LoadSettings();
        var port = request?.Port ?? settings.Port;
        var baudRate = request?.BaudRate ?? settings.BaudRate;

        var success = _serialPortService.Connect(port, baudRate);

        return Ok(new
        {
            success,
            message = success ? "Connected" : _serialPortService.PortInfo.Status,
            info = _serialPortService.PortInfo
        });
    }

    [HttpPost("disconnect")]
    public IActionResult Disconnect()
    {
        _serialPortService.Disconnect();
        return Ok(new { success = true, message = "Disconnected" });
    }

    [HttpPost("connect_quantity")]
    public IActionResult ConnectQuantity([FromBody] ConnectRequest request)
    {
        if (_serialPortService.IsConnectedQuantity)
            return BadRequest(new { success = false, message = "Already connected" });

        var settings = _settingsService.LoadSettings();
        var port = request?.PortQuantity ?? settings.PortQuantity;
        var baudRate = request?.BaudRateQuantity ?? settings.BaudRateQuantity;

        if (string.IsNullOrEmpty(port))
            return BadRequest(new { success = false, message = "Quantity COM port not configured" });

        var success = _serialPortService.ConnectQuantity(port, baudRate);

        return Ok(new
        {
            success,
            message = success ? "Connected" : "Connection failed",
            info = _serialPortService.PortInfoQuantity
        });
    }

    [HttpPost("disconnect_quantity")]
    public IActionResult DisconnectQuantity()
    {
        _serialPortService.DisconnectQuantity();
        return Ok(new { success = true, message = "Disconnected" });
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        var settings = _settingsService.LoadSettings();
        return Ok(settings);
    }

    [HttpPost("settings")]
    public IActionResult SaveSettings([FromBody] SettingsModel settings)
    {
        if (string.IsNullOrEmpty(settings.Port) || settings.BaudRate <= 0)
            return BadRequest(new { success = false, message = "port and baud_rate required" });

        var success = _settingsService.SaveSettings(settings);
        return Ok(new { success, settings });
    }

    [HttpGet("data")]
    public IActionResult GetData()
    {
        var data = _serialPortService.GetData();
        var quantityData = _serialPortService.GetQuantityData();

        return Ok(new
        {
            connected = _serialPortService.IsConnected,
            data,
            portInfo = _serialPortService.PortInfo,
            connectedQuantity = _serialPortService.IsConnectedQuantity,
            quantityData,
            portInfoQuantity = _serialPortService.PortInfoQuantity
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            connected = _serialPortService.IsConnected,
            portInfo = _serialPortService.PortInfo
        });
    }

    [HttpGet("ports")]
    public IActionResult GetPorts()
    {
        var ports = _serialPortService.GetAvailablePorts()
            .Select(p => new { port = p.Port, description = p.Description })
            .ToList();

        return Ok(new { ports });
    }

    [HttpGet("farmers")]
    public IActionResult GetFarmers()
    {
        var farmers = _farmersService.LoadFarmers();
        return Ok(farmers);
    }

    [HttpPost("farmers")]
    public IActionResult AddFarmer([FromBody] FarmerModel farmer)
    {
        if (string.IsNullOrEmpty(farmer.Code) || string.IsNullOrEmpty(farmer.Name))
            return BadRequest(new { success = false, message = "code and name required" });

        if (_farmersService.FarmerCodeExists(farmer.Code))
            return BadRequest(new { success = false, message = "code already exists" });

        var newFarmers = new List<FarmerModel> { farmer };
        var success = _farmersService.SaveFarmers(newFarmers);

        return Ok(new { success, farmer });
    }

    [HttpDelete("farmers/{code}")]
    public IActionResult DeleteFarmer(string code)
    {
        var success = _farmersService.DeleteFarmer(code);

        if (!success)
            return NotFound(new { success = false, message = "farmer not found" });

        return Ok(new { success, message = "farmer deleted" });
    }

    [HttpPut("farmers/{code}")]
    public IActionResult UpdateFarmer(string code, [FromBody] FarmerModel farmer)
    {
        var success = _farmersService.UpdateFarmer(code, farmer);

        if (!success)
            return NotFound(new { success = false, message = "farmer not found" });

        var updated = _farmersService.GetFarmerByCode(code);
        return Ok(new { success, farmer = updated });
    }

    [HttpPost("farmers/restore")]
    public IActionResult RestoreFarmers([FromBody] RestoreRequest request)
    {
        if (request?.Farmers == null || request.Farmers.Count == 0)
            return BadRequest(new { success = false, message = "Invalid backup data" });

        var success = _farmersService.RestoreFarmers(request.Farmers);
        return Ok(new
        {
            success,
            message = success ? $"Restored {request.Farmers.Count} farmers" : "Restore failed",
            count = request.Farmers.Count
        });
    }

    // ==================== MILK ENTRIES ENDPOINTS ====================

    /// <summary>
    /// Add a new milk entry
    /// POST /api/milk-entries
    /// </summary>
    [HttpPost("milk-entries")]
    public IActionResult AddMilkEntry([FromBody] AddMilkEntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FarmerCode))
            return BadRequest(new { success = false, message = "Farmer code is required" });

        if (request.Quantity <= 0)
            return BadRequest(new { success = false, message = "Quantity must be greater than zero" });

        var result = _milkEntryService.AddMilkEntry(request, User?.Identity?.Name);

        if (result == null)
            return BadRequest(new { success = false, message = "Failed to add milk entry" });

        return Ok(new { success = true, message = "Milk entry added successfully", entry = result });
    }

    /// <summary>
    /// Get all milk entries
    /// GET /api/milk-entries
    /// </summary>
    [HttpGet("milk-entries")]
    public IActionResult GetAllMilkEntries()
    {
        var entries = _milkEntryService.GetAllMilkEntries();
        return Ok(new { success = true, entries, count = entries.Count });
    }

    /// <summary>
    /// Get milk entries for a specific farmer
    /// GET /api/milk-entries/farmer/{farmerCode}
    /// </summary>
    [HttpGet("milk-entries/farmer/{farmerCode}")]
    public IActionResult GetMilkEntriesByFarmer(string farmerCode)
    {
        var entries = _milkEntryService.GetMilkEntriesByFarmer(farmerCode);
        return Ok(new { success = true, entries, count = entries.Count });
    }

    /// <summary>
    /// Get milk entries by date range
    /// GET /api/milk-entries/range?startDate=2025-12-22&endDate=2025-12-23
    /// </summary>
    [HttpGet("milk-entries/range")]
    public IActionResult GetMilkEntriesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (endDate < startDate)
            return BadRequest(new { success = false, message = "End date must be after start date" });

        var entries = _milkEntryService.GetMilkEntriesByDateRange(startDate, endDate);
        return Ok(new { success = true, entries, count = entries.Count });
    }

    /// <summary>
    /// Get a specific milk entry
    /// GET /api/milk-entries/{id}
    /// </summary>
    [HttpGet("milk-entries/{id}")]
    public IActionResult GetMilkEntryById(int id)
    {
        var entry = _milkEntryService.GetMilkEntryById(id);

        if (entry == null)
            return NotFound(new { success = false, message = "Milk entry not found" });

        return Ok(new { success = true, entry });
    }

    /// <summary>
    /// Update a milk entry
    /// PUT /api/milk-entries/{id}
    /// </summary>
    [HttpPut("milk-entries/{id}")]
    public IActionResult UpdateMilkEntry(int id, [FromBody] UpdateMilkEntryRequest request)
    {
        var success = _milkEntryService.UpdateMilkEntry(id, request);

        if (!success)
            return NotFound(new { success = false, message = "Milk entry not found" });

        var updated = _milkEntryService.GetMilkEntryById(id);
        return Ok(new { success = true, message = "Milk entry updated successfully", entry = updated });
    }

    /// <summary>
    /// Delete a milk entry
    /// DELETE /api/milk-entries/{id}
    /// </summary>
    [HttpDelete("milk-entries/{id}")]
    public IActionResult DeleteMilkEntry(int id)
    {
        var success = _milkEntryService.DeleteMilkEntry(id);

        if (!success)
            return NotFound(new { success = false, message = "Milk entry not found" });

        return Ok(new { success = true, message = "Milk entry deleted successfully" });
    }

    /// <summary>
    /// Confirm a milk entry
    /// PATCH /api/milk-entries/{id}/confirm
    /// </summary>
    [HttpPatch("milk-entries/{id}/confirm")]
    public IActionResult ConfirmMilkEntry(int id)
    {
        var success = _milkEntryService.ConfirmMilkEntry(id);

        if (!success)
            return NotFound(new { success = false, message = "Milk entry not found" });

        return Ok(new { success = true, message = "Milk entry confirmed" });
    }

    /// <summary>
    /// Reject a milk entry
    /// PATCH /api/milk-entries/{id}/reject
    /// </summary>
    [HttpPatch("milk-entries/{id}/reject")]
    public IActionResult RejectMilkEntry(int id, [FromQuery] string reason = "")
    {
        var success = _milkEntryService.RejectMilkEntry(id, reason);

        if (!success)
            return NotFound(new { success = false, message = "Milk entry not found" });

        return Ok(new { success = true, message = "Milk entry rejected" });
    }

    /// <summary>
    /// Get daily milk statistics
    /// GET /api/milk-statistics/daily?date=2025-12-22
    /// </summary>
    [HttpGet("milk-statistics/daily")]
    public IActionResult GetDailyStatistics([FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Now;
        var stats = _milkEntryService.GetDailyStatistics(targetDate);

        if (stats == null)
            return Ok(new { success = true, message = "No data for this date", stats = (object?)null });

        return Ok(new { success = true, stats });
    }

    /// <summary>
    /// Get milk statistics for a date range
    /// GET /api/milk-statistics/range?startDate=2025-12-22&endDate=2025-12-23
    /// </summary>
    [HttpGet("milk-statistics/range")]
    public IActionResult GetStatisticsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (endDate < startDate)
            return BadRequest(new { success = false, message = "End date must be after start date" });

        var stats = _milkEntryService.GetStatisticsByDateRange(startDate, endDate);
        return Ok(new { success = true, statistics = stats, count = stats.Count });
    }

    /// <summary>
    /// Get count of pending entries
    /// GET /api/milk-statistics/pending-count
    /// </summary>
    [HttpGet("milk-statistics/pending-count")]
    public IActionResult GetPendingEntriesCount()
    {
        var count = _milkEntryService.GetPendingEntriesCount();
        return Ok(new { success = true, pending_count = count });
    }
}

public class ConnectRequest
{
    public string? Port { get; set; }
    public int BaudRate { get; set; }
    public string? PortQuantity { get; set; }
    public int BaudRateQuantity { get; set; }
}

public class RestoreRequest
{
    public List<FarmerModel>? Farmers { get; set; }
}
