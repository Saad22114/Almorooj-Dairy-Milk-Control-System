using FarmersApp.Models;
using FarmersApp.Data;
using FarmersApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmersApp.Services;

/// <summary>
/// Service for managing milk entries
/// </summary>
public class MilkEntryService
{
    private readonly AppDbContext _context;

    public MilkEntryService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Add a new milk entry
    /// </summary>
    public MilkEntryModel? AddMilkEntry(AddMilkEntryRequest request, string? enteredBy = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FarmerCode))
                throw new ArgumentException("Farmer code is required");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            var entryDateTime = request.EntryDateTime ?? DateTime.Now;

            // Calculate the price
            decimal? calculatedPrice = null;
            if (request.Quantity > 0)
            {
                var settings = _context.Settings.FirstOrDefault(s => s.Id == 1);
                if (settings != null)
                {
                    decimal pricePerLiter = request.MilkType?.ToLower() == "camel"
                        ? settings.MilkPriceCamel
                        : settings.MilkPriceCow;
                    calculatedPrice = request.Quantity * pricePerLiter;
                }
            }

            var entity = new MilkEntryEntity
            {
                FarmerCode = request.FarmerCode,
                FarmerName = request.FarmerName,
                MilkType = request.MilkType ?? "cow",
                Quantity = request.Quantity,
                Temperature = request.Temperature,
                Density = request.Density,
                Quality = request.Quality,
                CalculatedPrice = calculatedPrice,
                Notes = request.Notes,
                Device = request.Device,
                EntryDateTime = entryDateTime,
                CreatedAt = DateTime.UtcNow,
                Status = "pending",
                EnteredBy = enteredBy
            };

            _context.MilkEntries.Add(entity);
            _context.SaveChanges();

            return MapEntityToModel(entity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding milk entry: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get all milk entries
    /// </summary>
    public List<MilkEntryModel> GetAllMilkEntries()
    {
        try
        {
            var entries = _context.MilkEntries
                .OrderByDescending(m => m.EntryDateTime)
                .ToList();

            return entries.Select(MapEntityToModel).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading milk entries: {ex.Message}");
            return new List<MilkEntryModel>();
        }
    }

    /// <summary>
    /// Get milk entries for a specific farmer
    /// </summary>
    public List<MilkEntryModel> GetMilkEntriesByFarmer(string farmerCode)
    {
        try
        {
            var entries = _context.MilkEntries
                .Where(m => m.FarmerCode == farmerCode)
                .OrderByDescending(m => m.EntryDateTime)
                .ToList();

            return entries.Select(MapEntityToModel).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading farmer milk entries: {ex.Message}");
            return new List<MilkEntryModel>();
        }
    }

    /// <summary>
    /// Get milk entries by date range
    /// </summary>
    public List<MilkEntryModel> GetMilkEntriesByDateRange(DateTime startDate, DateTime endDate)
    {
        try
        {
            var startDateTime = startDate.Date;
            var endDateTime = endDate.Date.AddDays(1);

            var entries = _context.MilkEntries
                .Where(m => m.EntryDateTime >= startDateTime && m.EntryDateTime < endDateTime)
                .OrderByDescending(m => m.EntryDateTime)
                .ToList();

            return entries.Select(MapEntityToModel).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading milk entries by date: {ex.Message}");
            return new List<MilkEntryModel>();
        }
    }

    /// <summary>
    /// Get a specific milk entry
    /// </summary>
    public MilkEntryModel? GetMilkEntryById(int id)
    {
        try
        {
            var entry = _context.MilkEntries.Find(id);
            return entry != null ? MapEntityToModel(entry) : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading milk entry: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update a milk entry
    /// </summary>
    public bool UpdateMilkEntry(int id, UpdateMilkEntryRequest request)
    {
        try
        {
            var entry = _context.MilkEntries.Find(id);
            if (entry == null)
                return false;

            if (!string.IsNullOrWhiteSpace(request.MilkType))
                entry.MilkType = request.MilkType;

            if (request.Quantity.HasValue && request.Quantity > 0)
                entry.Quantity = request.Quantity.Value;

            if (request.Temperature.HasValue)
                entry.Temperature = request.Temperature;

            if (request.Density.HasValue)
                entry.Density = request.Density;

            if (request.Quality.HasValue)
                entry.Quality = request.Quality;

            if (!string.IsNullOrWhiteSpace(request.Notes))
                entry.Notes = request.Notes;

            if (!string.IsNullOrWhiteSpace(request.Status))
                entry.Status = request.Status;

            if (!string.IsNullOrWhiteSpace(request.Device))
                entry.Device = request.Device;

            // إعادة حساب السعر
            if (request.Quantity.HasValue)
            {
                var settings = _context.Settings.FirstOrDefault(s => s.Id == 1);
                if (settings != null)
                {
                    decimal pricePerLiter = entry.MilkType?.ToLower() == "camel"
                        ? settings.MilkPriceCamel
                        : settings.MilkPriceCow;
                    entry.CalculatedPrice = entry.Quantity * pricePerLiter;
                }
            }

            entry.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating milk entry: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a milk entry
    /// </summary>
    public bool DeleteMilkEntry(int id)
    {
        try
        {
            var entry = _context.MilkEntries.Find(id);
            if (entry == null)
                return false;

            _context.MilkEntries.Remove(entry);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting milk entry: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get daily milk statistics
    /// </summary>
    public MilkStatisticsModel? GetDailyStatistics(DateTime date)
    {
        try
        {
            var startDateTime = date.Date;
            var endDateTime = startDateTime.AddDays(1);

            var entries = _context.MilkEntries
                .Where(m => m.EntryDateTime >= startDateTime && m.EntryDateTime < endDateTime)
                .ToList();

            if (entries.Count == 0)
                return null;

            var cowEntries = entries.Where(m => m.MilkType?.ToLower() == "cow").ToList();
            var camelEntries = entries.Where(m => m.MilkType?.ToLower() == "camel").ToList();

            return new MilkStatisticsModel
            {
                Date = date.Date,
                TotalQuantity = entries.Sum(m => m.Quantity),
                CowQuantity = cowEntries.Sum(m => m.Quantity),
                CamelQuantity = camelEntries.Sum(m => m.Quantity),
                EntryCount = entries.Count,
                FarmerCount = entries.Select(m => m.FarmerCode).Distinct().Count(),
                TotalPrice = entries.Sum(m => m.CalculatedPrice ?? 0),
                AverageQuality = entries.Where(m => m.Quality.HasValue).Average(m => (decimal?)m.Quality)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating daily statistics: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get statistics for a date range
    /// </summary>
    public List<MilkStatisticsModel> GetStatisticsByDateRange(DateTime startDate, DateTime endDate)
    {
        try
        {
            var statistics = new List<MilkStatisticsModel>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var stats = GetDailyStatistics(currentDate);
                if (stats != null)
                    statistics.Add(stats);

                currentDate = currentDate.AddDays(1);
            }

            return statistics;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating period statistics: {ex.Message}");
            return new List<MilkStatisticsModel>();
        }
    }

    /// <summary>
    /// Confirm a milk entry
    /// </summary>
    public bool ConfirmMilkEntry(int id)
    {
        try
        {
            var entry = _context.MilkEntries.Find(id);
            if (entry == null)
                return false;

            entry.Status = "confirmed";
            entry.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error confirming milk entry: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reject a milk entry
    /// </summary>
    public bool RejectMilkEntry(int id, string reason = "")
    {
        try
        {
            var entry = _context.MilkEntries.Find(id);
            if (entry == null)
                return false;

            entry.Status = "rejected";
            if (!string.IsNullOrEmpty(reason))
                entry.Notes = $"{entry.Notes}\n[Rejected: {reason}]";
            entry.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rejecting milk entry: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get count of pending entries
    /// </summary>
    public int GetPendingEntriesCount()
    {
        try
        {
            return _context.MilkEntries.Count(m => m.Status == "pending");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error counting pending entries: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Delete old milk entries (older than specified days)
    /// </summary>
    public int DeleteOldEntries(int daysOld = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var entriesToDelete = _context.MilkEntries
                .Where(m => m.CreatedAt < cutoffDate && m.Status == "confirmed")
                .ToList();

            _context.MilkEntries.RemoveRange(entriesToDelete);
            _context.SaveChanges();

            return entriesToDelete.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting old entries: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private MilkEntryModel MapEntityToModel(MilkEntryEntity entity)
    {
        return new MilkEntryModel
        {
            Id = entity.Id,
            FarmerCode = entity.FarmerCode,
            FarmerName = entity.FarmerName,
            MilkType = entity.MilkType,
            Quantity = entity.Quantity,
            Temperature = entity.Temperature,
            Density = entity.Density,
            Quality = entity.Quality,
            CalculatedPrice = entity.CalculatedPrice,
            Notes = entity.Notes,
            Status = entity.Status,
            Device = entity.Device,
            EntryDateTime = entity.EntryDateTime,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            EnteredBy = entity.EnteredBy
        };
    }
}
