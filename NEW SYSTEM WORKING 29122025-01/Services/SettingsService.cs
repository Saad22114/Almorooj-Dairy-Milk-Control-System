using FarmersApp.Models;
using FarmersApp.Data;
using FarmersApp.Data.Entities;
using System.Text.Json;

namespace FarmersApp.Services;

public class SettingsService
{
    private readonly AppDbContext _context;
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsService(AppDbContext context)
    {
        _context = context;
        _settingsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "settings.json");
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            WriteIndented = true 
        };
    }

    public SettingsModel LoadSettings()
    {
        try
        {
            var settings = _context.Settings.FirstOrDefault();
            
            // If database is empty, try to load from JSON file
            if (settings == null)
            {
                LoadFromJsonIfNeeded();
                settings = _context.Settings.FirstOrDefault();
            }

            if (settings != null)
            {
                return MapEntityToModel(settings);
            }

            return GetDefaultSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return GetDefaultSettings();
        }
    }

    public bool SaveSettings(SettingsModel settings)
    {
        try
        {
            var entity = _context.Settings.FirstOrDefault();
            
            if (entity == null)
            {
                entity = new SettingsEntity { Id = 1 };
                _context.Settings.Add(entity);
            }

            entity.Port = settings.Port ?? "COM1";
            entity.BaudRate = settings.BaudRate > 0 ? settings.BaudRate : 2400;
            entity.SensorMode = settings.SensorMode ?? "automatic";
            entity.PortQuantity = settings.PortQuantity;
            entity.BaudRateQuantity = settings.BaudRateQuantity > 0 ? settings.BaudRateQuantity : 9600;
            entity.QuantityMode = settings.QuantityMode ?? "manual";
            entity.MilkPrice = settings.MilkPrice;
            entity.MilkPriceCow = settings.MilkPriceCow;
            entity.MilkPriceCamel = settings.MilkPriceCamel;
            entity.Currency = settings.Currency ?? "OMR";
            entity.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            return false;
        }
    }

    private void LoadFromJsonIfNeeded()
    {
        try
        {
            var filePath = Path.GetFullPath(_settingsPath);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var settings = JsonSerializer.Deserialize<SettingsModel>(json, _jsonOptions);
                
                if (settings != null)
                {
                    var entity = new SettingsEntity
                    {
                        Id = 1,
                        Port = settings.Port ?? "COM1",
                        BaudRate = settings.BaudRate > 0 ? settings.BaudRate : 2400,
                        SensorMode = settings.SensorMode ?? "automatic",
                        PortQuantity = settings.PortQuantity,
                        BaudRateQuantity = settings.BaudRateQuantity > 0 ? settings.BaudRateQuantity : 9600,
                        QuantityMode = settings.QuantityMode ?? "manual",
                        MilkPrice = settings.MilkPrice,
                        MilkPriceCow = settings.MilkPriceCow,
                        MilkPriceCamel = settings.MilkPriceCamel,
                        Currency = settings.Currency ?? "OMR"
                    };
                    
                    _context.Settings.Add(entity);
                    _context.SaveChanges();
                    Console.WriteLine("Loaded settings from JSON file");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from JSON: {ex.Message}");
        }
    }

    private SettingsModel MapEntityToModel(SettingsEntity entity)
    {
        return new SettingsModel
        {
            Port = entity.Port,
            BaudRate = entity.BaudRate,
            SensorMode = entity.SensorMode,
            PortQuantity = entity.PortQuantity,
            BaudRateQuantity = entity.BaudRateQuantity,
            QuantityMode = entity.QuantityMode,
            MilkPrice = entity.MilkPrice,
            MilkPriceCow = entity.MilkPriceCow,
            MilkPriceCamel = entity.MilkPriceCamel,
            Currency = entity.Currency
        };
    }

    private SettingsModel GetDefaultSettings()
    {
        return new SettingsModel
        {
            Port = "COM1",
            BaudRate = 2400,
            SensorMode = "automatic",
            BaudRateQuantity = 9600,
            QuantityMode = "manual",
            MilkPrice = 0.0m,
            MilkPriceCow = 0.25m,
            MilkPriceCamel = 0.4m,
            Currency = "OMR"
        };
    }
}
