
# FarmersApp - Milk & Farmers Management System

C# ASP.NET Core web application for controlling milk sensors and managing farmer data.

## Requirements
- .NET 8.0 SDK or higher
- Windows with COM ports (for hardware)

## Installation and Running

### Method 1: Using PowerShell
```powershell
dotnet restore
dotnet run
```

### Method 2: Using Visual Studio
1. Open `FarmersApp.csproj`
2. Press F5 or click Run

## Access the Application
Open browser: **http://localhost:5000**

## Core Features

### 1. Device Connection
- Select COM port and baud rate
- Read milk data (Fat, SNF, Density, Water, Protein)
- Read quantity data (scale/weight)

### 2. Farmer Management
- Add new farmers
- Update farmer data
- Delete farmers
- Restore from backups

### 3. Settings
- Manage COM ports
- Set milk prices (general, cow, camel)
- Configure currency

### 4. Reports
- Farmer reports
- Data analysis tools

## Project Structure


FarmersApp/
├── Controllers/       # API and page handlers
├── Models/           # Data models
├── Services/         # Business logic (Serial Port, Settings, Farmers)
├── Views/            # HTML pages
├── Program.cs        # Application entry point
├── FarmersApp.csproj # Project file
└── appsettings.json  # Configuration


## API Endpoints

 Connection

POST   /api/connect              # Connect to sensor
POST   /api/disconnect           # Disconnect
POST   /api/connect_quantity     # Connect to weight sensor
POST   /api/disconnect_quantity  # Disconnect weight sensor
GET    /api/status               # Connection status
GET    /api/ports                # Available COM ports
```

### Data
```
GET    /api/data                 # Get all sensor data
```

### Settings
```
GET    /api/settings             # Get settings
POST   /api/settings             # Save settings
```

### Farmers
```
GET    /api/farmers              # Get all farmers
POST   /api/farmers              # Add new farmer
PUT    /api/farmers/{code}       # Update farmer
DELETE /api/farmers/{code}       # Delete farmer
POST   /api/farmers/restore      # Restore from backup
```

## Version
1.0.0

## License
All rights reserved
=======
# Almorooj-Dairy-Milk-Control-System
Almorooj Dairy Milk Control System
 a1218676590f3611ec26d7f7931482ea30ba96cc
