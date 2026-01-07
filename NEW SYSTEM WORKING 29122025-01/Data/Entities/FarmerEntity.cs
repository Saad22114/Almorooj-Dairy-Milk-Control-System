using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmersApp.Data.Entities;

[Table("Farmers")]
public class FarmerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? Nid { get; set; }

    [StringLength(50)]
    public string? Type { get; set; } // COW, CAMEL

    [StringLength(100)]
    public string? Center { get; set; }

    [StringLength(100)]
    public string? Area { get; set; }

    [StringLength(150)]
    public string? Bank { get; set; }

    [StringLength(50)]
    public string? BankAcc { get; set; }

    [StringLength(20)]
    public string? BankSwift { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    public int AnimalCount { get; set; }

    public int ExpectedQty { get; set; }

    public int Maximum { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "active"; // active, inactive

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
