using System.Text.Json.Serialization;

namespace FarmersApp.Models;

/// <summary>
/// Milk Entry Data Transfer Object
/// </summary>
public class MilkEntryModel
{
    /// <summary>Entry Id</summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>Farmer Code</summary>
    [JsonPropertyName("farmer_code")]
    public string FarmerCode { get; set; } = string.Empty;

    /// <summary>Farmer Name</summary>
    [JsonPropertyName("farmer_name")]
    public string? FarmerName { get; set; }

    /// <summary>Milk Type (cow/camel)</summary>
    [JsonPropertyName("milk_type")]
    public string MilkType { get; set; } = "cow";

    /// <summary>Milk Quantity (in liters)</summary>
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    /// <summary>Temperature</summary>
    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    /// <summary>Density</summary>
    [JsonPropertyName("density")]
    public decimal? Density { get; set; }

    /// <summary>Milk Quality (1-5)</summary>
    [JsonPropertyName("quality")]
    public int? Quality { get; set; }

    /// <summary>Calculated Price</summary>
    [JsonPropertyName("calculated_price")]
    public decimal? CalculatedPrice { get; set; }

    /// <summary>Notes</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    /// <summary>Entry Status</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "pending";

    /// <summary>Device Used</summary>
    [JsonPropertyName("device")]
    public string? Device { get; set; }

    /// <summary>Entry Date and Time</summary>
    [JsonPropertyName("entry_date_time")]
    public DateTime EntryDateTime { get; set; }

    /// <summary>Creation Date and Time</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last Update Date and Time</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Who Entered the Data</summary>
    [JsonPropertyName("entered_by")]
    public string? EnteredBy { get; set; }
}

/// <summary>
/// Request model to add a new milk entry
/// </summary>
public class AddMilkEntryRequest
{
    [JsonPropertyName("farmer_code")]
    public string FarmerCode { get; set; } = string.Empty;

    [JsonPropertyName("farmer_name")]
    public string? FarmerName { get; set; }

    [JsonPropertyName("milk_type")]
    public string MilkType { get; set; } = "cow";

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    [JsonPropertyName("density")]
    public decimal? Density { get; set; }

    [JsonPropertyName("quality")]
    public int? Quality { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("entry_date_time")]
    public DateTime? EntryDateTime { get; set; }

    [JsonPropertyName("entered_by")]
    public string? EnteredBy { get; set; }
}

/// <summary>
/// Request model to update a milk entry
/// </summary>
public class UpdateMilkEntryRequest
{
    [JsonPropertyName("milk_type")]
    public string? MilkType { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    [JsonPropertyName("density")]
    public decimal? Density { get; set; }

    [JsonPropertyName("quality")]
    public int? Quality { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }
}

/// <summary>
/// Daily milk statistics model
/// </summary>
public class MilkStatisticsModel
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("total_quantity")]
    public decimal TotalQuantity { get; set; }

    [JsonPropertyName("cow_quantity")]
    public decimal CowQuantity { get; set; }

    [JsonPropertyName("camel_quantity")]
    public decimal CamelQuantity { get; set; }

    [JsonPropertyName("entry_count")]
    public int EntryCount { get; set; }

    [JsonPropertyName("farmer_count")]
    public int FarmerCount { get; set; }

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("average_quality")]
    public decimal? AverageQuality { get; set; }
}
