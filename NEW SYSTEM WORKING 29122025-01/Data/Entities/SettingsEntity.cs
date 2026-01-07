using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmersApp.Data.Entities;

[Table("Settings")]
public class SettingsEntity
{
    [Key]
    public int Id { get; set; } = 1; // Only one record

    [StringLength(50)]
    public string Port { get; set; } = "COM1";

    public int BaudRate { get; set; } = 2400;

    [StringLength(50)]
    public string SensorMode { get; set; } = "automatic";

    [StringLength(50)]
    public string? PortQuantity { get; set; }

    public int BaudRateQuantity { get; set; } = 9600;

    [StringLength(50)]
    public string QuantityMode { get; set; } = "manual";

    [Column(TypeName = "decimal(18,2)")]
    public decimal MilkPrice { get; set; } = 0.0m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MilkPriceCow { get; set; } = 0.25m;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MilkPriceCamel { get; set; } = 0.4m;

    [StringLength(10)]
    public string Currency { get; set; } = "OMR";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
