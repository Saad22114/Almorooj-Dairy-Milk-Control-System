using System.Text.Json.Serialization;

namespace FarmersApp.Models;

public class SettingsModel
{
    [JsonPropertyName("port")]
    public string Port { get; set; } = "COM1";

    [JsonPropertyName("baud_rate")]
    public int BaudRate { get; set; } = 2400;

    [JsonPropertyName("sensor_mode")]
    public string SensorMode { get; set; } = "automatic";

    [JsonPropertyName("port_quantity")]
    public string? PortQuantity { get; set; }

    [JsonPropertyName("baud_rate_quantity")]
    public int BaudRateQuantity { get; set; } = 9600;

    [JsonPropertyName("quantity_mode")]
    public string QuantityMode { get; set; } = "manual";

    [JsonPropertyName("milk_price")]
    public decimal MilkPrice { get; set; } = 0.0m;

    [JsonPropertyName("milk_price_cow")]
    public decimal MilkPriceCow { get; set; } = 0.25m;

    [JsonPropertyName("milk_price_camel")]
    public decimal MilkPriceCamel { get; set; } = 0.4m;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "OMR";
}
