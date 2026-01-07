using FarmersApp.Models;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace FarmersApp.Services;

public class SerialPortService
{
    private SerialPort? _serialPort;
    private SerialPort? _serialPortQuantity;
    private bool _isConnected;
    private bool _isConnectedQuantity;
    private PortInfoModel _portInfo = new();
    private PortInfoModel _portInfoQuantity = new();
    private readonly ConcurrentQueue<DataResponseModel> _dataQueue = new();
    private readonly ConcurrentQueue<QuantityDataModel> _quantityQueue = new();

    public bool IsConnected => _isConnected;
    public bool IsConnectedQuantity => _isConnectedQuantity;
    public PortInfoModel PortInfo => _portInfo;
    public PortInfoModel PortInfoQuantity => _portInfoQuantity;

    public List<DataResponseModel> GetData()
    {
        var dataList = new List<DataResponseModel>();
        while (_dataQueue.TryDequeue(out var data))
        {
            dataList.Add(data);
        }
        return dataList;
    }

    public QuantityDataModel? GetQuantityData()
    {
        QuantityDataModel? lastData = null;
        while (_quantityQueue.TryDequeue(out var data))
        {
            lastData = data;
        }
        return lastData;
    }

    public bool Connect(string port, int baudRate)
    {
        if (_isConnected)
            return false;

        try
        {
            _serialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000
            };
            _serialPort.Open();
            _isConnected = true;
            _portInfo = new PortInfoModel { Port = port, BaudRate = baudRate, Status = "Connected" };

            // Start reading in background
            Task.Run(() => ReadSerialData());

            return true;
        }
        catch (Exception ex)
        {
            _isConnected = false;
            var errorMsg = ex.Message;
            if (errorMsg.Contains("Access is denied") || errorMsg.Contains("already in use"))
                errorMsg = "Port is in use or inaccessible.";
            else if (errorMsg.Contains("could not open"))
                errorMsg = $"Port not found. {errorMsg}";

            _portInfo = new PortInfoModel { Port = port, BaudRate = baudRate, Status = $"Error: {errorMsg}" };
            Console.WriteLine($"Serial Connection Failed: {errorMsg}");
            return false;
        }
    }

    public bool ConnectQuantity(string port, int baudRate)
    {
        if (_isConnectedQuantity)
            return false;

        try
        {
            _serialPortQuantity = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000
            };
            _serialPortQuantity.Open();
            _isConnectedQuantity = true;
            _portInfoQuantity = new PortInfoModel { Port = port, BaudRate = baudRate, Status = "Connected" };

            // Start reading in background
            Task.Run(() => ReadSerialDataQuantity());

            return true;
        }
        catch (Exception ex)
        {
            _isConnectedQuantity = false;
            _portInfoQuantity = new PortInfoModel 
            { 
                Port = port, 
                BaudRate = baudRate, 
                Status = $"Error: {ex.Message}" 
            };
            Console.WriteLine($"Quantity Connection Failed: {ex.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _serialPort?.Close();
        _serialPort?.Dispose();
        _serialPort = null;
    }

    public void DisconnectQuantity()
    {
        _isConnectedQuantity = false;
        _serialPortQuantity?.Close();
        _serialPortQuantity?.Dispose();
        _serialPortQuantity = null;
    }

    public List<(string Port, string Description)> GetAvailablePorts()
    {
        var ports = new List<(string, string)>();
        try
        {
            var portNames = SerialPort.GetPortNames();
            foreach (var port in portNames)
            {
                ports.Add((port, $"{port} - Serial Port"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting ports: {ex.Message}");
        }
        return ports;
    }

    private void ReadSerialData()
    {
        while (_isConnected && _serialPort != null)
        {
            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    var data = _serialPort.ReadExisting();
                    var timestamp = DateTime.UtcNow.ToString("o");
                    var parsed = ParseSensorData(data);

                    _dataQueue.Enqueue(new DataResponseModel
                    {
                        Time = timestamp,
                        RawData = data,
                        Parsed = parsed
                    });
                }
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading serial data: {ex.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    private void ReadSerialDataQuantity()
    {
        while (_isConnectedQuantity && _serialPortQuantity != null)
        {
            try
            {
                if (_serialPortQuantity.BytesToRead > 0)
                {
                    var data = _serialPortQuantity.ReadLine();
                    var timestamp = DateTime.UtcNow.ToString("o");
                    var parsed = ParseQuantityData(data);

                    _quantityQueue.Enqueue(new QuantityDataModel
                    {
                        Time = timestamp,
                        RawData = data,
                        Parsed = parsed
                    });
                }
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading quantity data: {ex.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    private SensorDataModel? ParseSensorData(string rawData)
    {
        try
        {
            var clean = new string(rawData.Where(char.IsDigit).ToArray());

            if (clean.Length < 23)
                return null;

            var fat = double.Parse(clean.Substring(0, 4)) / 100.0;
            var snf = double.Parse(clean.Substring(4, 4)) / 100.0;
            var density = clean.Substring(8, 5);
            var water = double.Parse(clean.Substring(13, 5)) / 1000.0;
            var protein = double.Parse(clean.Substring(18, 5)) / 1000.0;

            return new SensorDataModel
            {
                Fat = Math.Round(fat, 2),
                Snf = Math.Round(snf, 2),
                Density = density,
                AddedWater = Math.Round(water, 2),
                Protein = Math.Round(protein, 2),
                Raw = rawData
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing sensor data: {ex.Message}");
            return null;
        }
    }

    private double? ParseQuantityData(string rawData)
    {
        try
        {
            var clean = new string(rawData.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (double.TryParse(clean, out var value))
            {
                if (!rawData.Contains(".") && value >= 10)
                    value /= 10;

                return Math.Round(value, 2);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing quantity data: {ex.Message}");
            return null;
        }
    }
}
