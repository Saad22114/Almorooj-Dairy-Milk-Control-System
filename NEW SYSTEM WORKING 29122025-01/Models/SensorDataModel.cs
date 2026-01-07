namespace FarmersApp.Models;

public class SensorDataModel
{
    public double? Fat { get; set; }
    public double? Snf { get; set; }
    public string? Density { get; set; }
    public double? AddedWater { get; set; }
    public double? Protein { get; set; }
    public string? Raw { get; set; }
}

public class DataResponseModel
{
    public string? Time { get; set; }
    public string? RawData { get; set; }
    public SensorDataModel? Parsed { get; set; }
}

public class QuantityDataModel
{
    public string? Time { get; set; }
    public string? RawData { get; set; }
    public double? Parsed { get; set; }
}

public class PortInfoModel
{
    public string? Port { get; set; }
    public int BaudRate { get; set; }
    public string? Status { get; set; }
}
