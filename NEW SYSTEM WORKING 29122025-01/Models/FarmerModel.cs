using System.Text.Json.Serialization;

namespace FarmersApp.Models;

public class FarmerModel
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("nid")]
    public string? Nid { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; } // COW, CAMEL

    [JsonPropertyName("center")]
    public string? Center { get; set; }

    [JsonPropertyName("area")]
    public string? Area { get; set; }

    [JsonPropertyName("bank")]
    public string? Bank { get; set; }

    [JsonPropertyName("bank_acc")]
    public string? BankAcc { get; set; }

    [JsonPropertyName("bank_swift")]
    public string? BankSwift { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("animal_count")]
    public int? AnimalCount { get; set; }

    [JsonPropertyName("expected_qty")]
    public int? ExpectedQty { get; set; }

    [JsonPropertyName("maximum")]
    public int? Maximum { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
