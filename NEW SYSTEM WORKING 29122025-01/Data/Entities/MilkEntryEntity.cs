using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FarmersApp.Data.Entities;

/// <summary>
/// Entity for recording milk entries
/// </summary>
[Table("MilkEntries")]
public class MilkEntryEntity
{
    /// <summary>Unique entry identifier</summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Farmer Code</summary>
    [Required]
    [StringLength(50)]
    public string FarmerCode { get; set; } = string.Empty;

    /// <summary>Farmer Name</summary>
    [StringLength(255)]
    public string? FarmerName { get; set; }

    /// <summary>Milk Type (cow/camel)</summary>
    [Required]
    [StringLength(50)]
    public string MilkType { get; set; } = "cow";

    /// <summary>Milk Quantity (in liters)</summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    /// <summary>Temperature (optional)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Temperature { get; set; }

    /// <summary>Density (optional)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Density { get; set; }

    /// <summary>Milk Quality (1-5)</summary>
    public int? Quality { get; set; }

    /// <summary>Calculated Price</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CalculatedPrice { get; set; }

    /// <summary>Notes</summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>Entry Status (pending, confirmed, rejected)</summary>
    [StringLength(50)]
    public string Status { get; set; } = "pending";

    /// <summary>Device/Sensor Used</summary>
    [StringLength(50)]
    public string? Device { get; set; }

    /// <summary>Entry Date and Time</summary>
    [Required]
    public DateTime EntryDateTime { get; set; }

    /// <summary>Creation Date and Time in Database</summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last Update Date and Time</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Who Entered the Data (username/device)</summary>
    [StringLength(100)]
    public string? EnteredBy { get; set; }
}
