using FarmersApp.Models;
using FarmersApp.Data;
using FarmersApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FarmersApp.Services;

public class FarmersService
{
    private readonly AppDbContext _context;
    private readonly string _farmersPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public FarmersService(AppDbContext context)
    {
        _context = context;
        _farmersPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "farmers.json");
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public List<FarmerModel> LoadFarmers()
    {
        try
        {
            var farmers = _context.Farmers.OrderBy(f => f.Code).ToList();
            
            // If database is empty, try to load from JSON file
            if (farmers.Count == 0)
            {
                LoadFromJsonIfNeeded();
                farmers = _context.Farmers.OrderBy(f => f.Code).ToList();
            }

            return farmers.Select(MapEntityToModel).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading farmers: {ex.Message}");
            return new List<FarmerModel>();
        }
    }

    public bool SaveFarmers(List<FarmerModel> farmers)
    {
        try
        {
            foreach (var farmer in farmers)
            {
                var existingFarmer = _context.Farmers.Find(farmer.Code);
                
                if (existingFarmer == null)
                {
                    var entity = MapModelToEntity(farmer);
                    _context.Farmers.Add(entity);
                }
                else
                {
                    existingFarmer.Name = farmer.Name ?? existingFarmer.Name;
                    existingFarmer.Phone = farmer.Phone ?? existingFarmer.Phone;
                    existingFarmer.Nid = farmer.Nid ?? existingFarmer.Nid;
                    existingFarmer.Type = farmer.Type ?? existingFarmer.Type;
                    existingFarmer.Center = farmer.Center ?? existingFarmer.Center;
                    existingFarmer.Area = farmer.Area ?? existingFarmer.Area;
                    existingFarmer.Bank = farmer.Bank ?? existingFarmer.Bank;
                    existingFarmer.BankAcc = farmer.BankAcc ?? existingFarmer.BankAcc;
                    existingFarmer.BankSwift = farmer.BankSwift ?? existingFarmer.BankSwift;
                    existingFarmer.Address = farmer.Address ?? existingFarmer.Address;
                    if (farmer.AnimalCount.HasValue && farmer.AnimalCount > 0) existingFarmer.AnimalCount = farmer.AnimalCount.Value;
                    if (farmer.ExpectedQty.HasValue && farmer.ExpectedQty > 0) existingFarmer.ExpectedQty = farmer.ExpectedQty.Value;
                    if (farmer.Maximum.HasValue && farmer.Maximum > 0) existingFarmer.Maximum = farmer.Maximum.Value;
                    existingFarmer.Status = farmer.Status ?? existingFarmer.Status;
                    existingFarmer.UpdatedAt = DateTime.UtcNow;
                }
            }

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving farmers: {ex.Message}");
            return false;
        }
    }

    public bool FarmerCodeExists(string code)
    {
        return _context.Farmers.Any(f => f.Code == code);
    }

    public FarmerModel? GetFarmerByCode(string code)
    {
        var farmer = _context.Farmers.Find(code);
        return farmer != null ? MapEntityToModel(farmer) : null;
    }

    public bool DeleteFarmer(string code)
    {
        try
        {
            var farmer = _context.Farmers.Find(code);
            if (farmer == null)
                return false;

            _context.Farmers.Remove(farmer);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting farmer: {ex.Message}");
            return false;
        }
    }

    public bool UpdateFarmer(string code, FarmerModel updated)
    {
        try
        {
            var farmer = _context.Farmers.Find(code);
            if (farmer == null)
                return false;

            farmer.Name = updated.Name ?? farmer.Name;
            farmer.Phone = updated.Phone ?? farmer.Phone;
            farmer.Nid = updated.Nid ?? farmer.Nid;
            farmer.Type = updated.Type ?? farmer.Type;
            farmer.Center = updated.Center ?? farmer.Center;
            farmer.Area = updated.Area ?? farmer.Area;
            farmer.Bank = updated.Bank ?? farmer.Bank;
            farmer.BankAcc = updated.BankAcc ?? farmer.BankAcc;
            farmer.BankSwift = updated.BankSwift ?? farmer.BankSwift;
            farmer.Address = updated.Address ?? farmer.Address;
            if (updated.AnimalCount.HasValue) farmer.AnimalCount = updated.AnimalCount.Value;
            if (updated.ExpectedQty.HasValue) farmer.ExpectedQty = updated.ExpectedQty.Value;
            if (updated.Maximum.HasValue) farmer.Maximum = updated.Maximum.Value;
            farmer.Status = updated.Status ?? farmer.Status;
            farmer.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating farmer: {ex.Message}");
            return false;
        }
    }

    public bool RestoreFarmers(List<FarmerModel> farmersData)
    {
        try
        {
            if (farmersData == null || farmersData.Count == 0)
                return false;

            foreach (var farmerModel in farmersData)
            {
                var existingFarmer = _context.Farmers.Find(farmerModel.Code);
                
                if (existingFarmer == null)
                {
                    var entity = MapModelToEntity(farmerModel);
                    _context.Farmers.Add(entity);
                }
            }

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring farmers: {ex.Message}");
            return false;
        }
    }

    private void LoadFromJsonIfNeeded()
    {
        try
        {
            var filePath = Path.GetFullPath(_farmersPath);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                var farmers = JsonSerializer.Deserialize<List<FarmerModel>>(json, _jsonOptions);
                
                if (farmers != null && farmers.Count > 0)
                {
                    foreach (var farmer in farmers)
                    {
                        if (!string.IsNullOrEmpty(farmer.Code) && !FarmerCodeExists(farmer.Code))
                        {
                            var entity = MapModelToEntity(farmer);
                            _context.Farmers.Add(entity);
                        }
                    }
                    _context.SaveChanges();
                    Console.WriteLine($"Loaded {farmers.Count} farmers from JSON file");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from JSON: {ex.Message}");
        }
    }

    private FarmerModel MapEntityToModel(FarmerEntity entity)
    {
        return new FarmerModel
        {
            Code = entity.Code,
            Name = entity.Name,
            Phone = entity.Phone,
            Nid = entity.Nid,
            Type = entity.Type,
            Center = entity.Center,
            Area = entity.Area,
            Bank = entity.Bank,
            BankAcc = entity.BankAcc,
            BankSwift = entity.BankSwift,
            Address = entity.Address,
            AnimalCount = entity.AnimalCount,
            ExpectedQty = entity.ExpectedQty,
            Maximum = entity.Maximum,
            Status = entity.Status
        };
    }

    private FarmerEntity MapModelToEntity(FarmerModel model)
    {
        return new FarmerEntity
        {
            Code = model.Code ?? string.Empty,
            Name = model.Name ?? string.Empty,
            Phone = model.Phone,
            Nid = model.Nid,
            Type = model.Type,
            Center = model.Center,
            Area = model.Area,
            Bank = model.Bank,
            BankAcc = model.BankAcc,
            BankSwift = model.BankSwift,
            Address = model.Address,
            AnimalCount = model.AnimalCount ?? 0,
            ExpectedQty = model.ExpectedQty ?? 0,
            Maximum = model.Maximum ?? 0,
            Status = model.Status ?? "active"
        };
    }

    public List<FarmerModel> LoadFarmersOld()
    {
        try
        {
            if (File.Exists(_farmersPath))
            {
                var json = File.ReadAllText(_farmersPath, System.Text.Encoding.UTF8);
                var farmers = JsonSerializer.Deserialize<List<FarmerModel>>(json, _jsonOptions);
                return farmers ?? new List<FarmerModel>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading farmers: {ex.Message}");
        }

        return new List<FarmerModel>();
    }

    public bool SaveFarmersOld(List<FarmerModel> farmers)
    {
        try
        {
            var json = JsonSerializer.Serialize(farmers, _jsonOptions);
            File.WriteAllText(_farmersPath, json, System.Text.Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving farmers: {ex.Message}");
            return false;
        }
    }

    public bool FarmerCodeExistsOld(string code)
    {
        var farmers = LoadFarmersOld();
        return farmers.Any(f => f.Code == code);
    }

    public FarmerModel? GetFarmerByCodeOld(string code)
    {
        var farmers = LoadFarmersOld();
        return farmers.FirstOrDefault(f => f.Code == code);
    }

    public bool DeleteFarmerOld(string code)
    {
        var farmers = LoadFarmersOld();
        var farmer = farmers.FirstOrDefault(f => f.Code == code);
        
        if (farmer == null)
            return false;

        farmers.Remove(farmer);
        return SaveFarmersOld(farmers);
    }

    public bool UpdateFarmerOld(string code, FarmerModel updated)
    {
        var farmers = LoadFarmersOld();
        var farmer = farmers.FirstOrDefault(f => f.Code == code);
        
        if (farmer == null)
            return false;

        // Update properties
        farmer.Name = updated.Name ?? farmer.Name;
        farmer.Phone = updated.Phone ?? farmer.Phone;
        farmer.Nid = updated.Nid ?? farmer.Nid;
        farmer.Type = updated.Type ?? farmer.Type;
        farmer.Center = updated.Center ?? farmer.Center;
        farmer.Area = updated.Area ?? farmer.Area;
        farmer.Bank = updated.Bank ?? farmer.Bank;
        farmer.BankAcc = updated.BankAcc ?? farmer.BankAcc;
        farmer.BankSwift = updated.BankSwift ?? farmer.BankSwift;
        farmer.Address = updated.Address ?? farmer.Address;
        if (updated.AnimalCount.HasValue) farmer.AnimalCount = updated.AnimalCount;
        if (updated.ExpectedQty.HasValue) farmer.ExpectedQty = updated.ExpectedQty;
        if (updated.Maximum.HasValue) farmer.Maximum = updated.Maximum;
        farmer.Status = updated.Status ?? farmer.Status;

        return SaveFarmersOld(farmers);
    }

    public bool RestoreFarmersOld(List<FarmerModel> farmersData)
    {
        if (farmersData == null || farmersData.Count == 0)
            return false;

        // Backup current data
        var currentFarmers = LoadFarmersOld();
        if (currentFarmers.Count > 0)
        {
            try
            {
                var backupPath = Path.Combine(
                    AppContext.BaseDirectory,
                    $"farmers_backup_before_restore_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                );
                var backupJson = JsonSerializer.Serialize(currentFarmers, _jsonOptions);
                File.WriteAllText(backupPath, backupJson, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create backup: {ex.Message}");
            }
        }

        return SaveFarmersOld(farmersData);
    }
}
