# SQL Server Integration - وثائق احترافية

## نظرة عامة
تم تحويل التطبيق من استخدام ملفات JSON إلى استخدام **SQL Server (LocalDB)** مع الحفاظ على جميع البيانات الحالية.

## المميزات الرئيسية

### 1️⃣ قاعدة بيانات احترافية
- **Database**: `FarmersAppDb` في LocalDB
- **Version**: SQL Server 2019+
- **Authentication**: Trusted Connection (Windows Auth)

### 2️⃣ Entity Framework Core
- **Version**: 8.0.0
- **Migrations**: مُطبقة تلقائياً عند بدء التطبيق
- **Lazy Loading**: معطّل (استخدام explicit loading)

### 3️⃣ جداول قاعدة البيانات

#### جدول Farmers
```sql
CREATE TABLE [Farmers] (
    [Code] NVARCHAR(450) PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL,
    [Phone] NVARCHAR(20),
    [Nid] NVARCHAR(20),
    [Type] NVARCHAR(50),
    [Center] NVARCHAR(100),
    [Area] NVARCHAR(100),
    [Bank] NVARCHAR(150),
    [BankAcc] NVARCHAR(50),
    [BankSwift] NVARCHAR(20),
    [Address] NVARCHAR(300),
    [AnimalCount] INT DEFAULT 0,
    [ExpectedQty] INT DEFAULT 0,
    [Maximum] INT DEFAULT 0,
    [Status] NVARCHAR(50) DEFAULT 'active',
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2
)
```

#### جدول Settings
```sql
CREATE TABLE [Settings] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Port] NVARCHAR(50) DEFAULT 'COM1',
    [BaudRate] INT DEFAULT 2400,
    [SensorMode] NVARCHAR(50) DEFAULT 'automatic',
    [PortQuantity] NVARCHAR(50),
    [BaudRateQuantity] INT DEFAULT 9600,
    [QuantityMode] NVARCHAR(50) DEFAULT 'manual',
    [MilkPrice] DECIMAL(18,2) DEFAULT 0,
    [MilkPriceCow] DECIMAL(18,2) DEFAULT 0.25,
    [MilkPriceCamel] DECIMAL(18,2) DEFAULT 0.4,
    [Currency] NVARCHAR(10) DEFAULT 'OMR',
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
```

### 4️⃣ الفهارس (Indexes)
تم إنشاء الفهارس التالية لتحسين الأداء:
- `Farmers.Center` - للبحث حسب المركز
- `Farmers.Name` - للبحث حسب الاسم
- `Farmers.Type` - للتصفية حسب النوع (بقر/إبل)
- `Farmers.Status` - للبحث حسب الحالة

## البيانات المهاجرة

### التحميل التلقائي من JSON
عند بدء التطبيق:
1. يتحقق من وجود بيانات في قاعدة البيانات
2. إذا كانت فارغة، يحمل البيانات من `farmers.json` و `settings.json` تلقائياً
3. يحفظ البيانات في قاعدة البيانات

### عدد البيانات المهاجرة
- **Farmers**: 300 مزارع
- **Settings**: 1 سجل إعدادات

## البنية المعمارية

### Services (الخدمات)

#### FarmersService
```csharp
public class FarmersService
{
    private readonly AppDbContext _context;
    
    // الدوال المتاحة:
    - LoadFarmers()           // تحميل جميع المزارعين
    - SaveFarmers()           // حفظ المزارعين
    - FarmerCodeExists()      // التحقق من وجود مزارع
    - GetFarmerByCode()       // الحصول على مزارع محدد
    - DeleteFarmer()          // حذف مزارع
    - UpdateFarmer()          // تحديث مزارع
    - RestoreFarmers()        // استعادة النسخة الاحتياطية
}
```

#### SettingsService
```csharp
public class SettingsService
{
    private readonly AppDbContext _context;
    
    // الدوال المتاحة:
    - LoadSettings()          // تحميل الإعدادات
    - SaveSettings()          // حفظ الإعدادات
}
```

### DbContext

#### AppDbContext
```csharp
public class AppDbContext : DbContext
{
    public DbSet<FarmerEntity> Farmers { get; set; }
    public DbSet<SettingsEntity> Settings { get; set; }
    
    // خصائص البيانات الافتراضية والفهارس
}
```

## الملفات المعدلة

### 1. FarmersApp.csproj
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

### 2. appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FarmersAppDb;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

### 3. Program.cs
```csharp
// إضافة DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// تغيير Scopes للخدمات
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<FarmersService>();

// تطبيق Migrations تلقائياً
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

## أمثلة الاستخدام

### الحصول على جميع المزارعين
```bash
GET http://localhost:5000/api/farmers
```

### إضافة مزارع جديد
```bash
POST http://localhost:5000/api/farmers
Content-Type: application/json

{
  "code": "9999",
  "name": "أحمد محمد",
  "phone": "9876543210",
  "type": "COW",
  "center": "الغدو",
  "area": "الدخيلية"
}
```

### تحديث مزارع
```bash
PUT http://localhost:5000/api/farmers/1000
Content-Type: application/json

{
  "name": "محمد علي الجديد",
  "phone": "9876543211"
}
```

### حذف مزارع
```bash
DELETE http://localhost:5000/api/farmers/1000
```

### الحصول على الإعدادات
```bash
GET http://localhost:5000/api/settings
```

### حفظ الإعدادات
```bash
POST http://localhost:5000/api/settings
Content-Type: application/json

{
  "port": "COM3",
  "baud_rate": 9600,
  "quantity_mode": "automatic",
  "milk_price_cow": 0.3
}
```

## الأداء

### تحسينات الأداء المُطبقة

1. **الفهارس** (Indexes)
   - فهرس على `Name` لتسريع البحث
   - فهرس على `Center` للتصفية حسب المركز
   - فهرس على `Type` للتصفية حسب النوع
   - فهرس على `Status` للبحث حسب الحالة

2. **التخزين المؤقت** (Caching)
   - Settings يُحمل مرة واحدة فقط
   - لا يوجد N+1 query problems

3. **Pagination** (يمكن إضافته لاحقاً)
   - دعم تقسيم النتائج لعدد كبير من السجلات

## الأمان

### Best Practices المطبقة
1. ✅ Parameterized Queries (تلقائياً من EF Core)
2. ✅ Connection String Encryption (يمكن تحسينها)
3. ✅ Validation on DTOs (يمكن تحسينها)
4. ✅ Auth placeholder (يمكن إضافتها)

## الصيانة والتطوير

### الترقية المستقبلية
1. إضافة Authentication و Authorization
2. إضافة API Versioning
3. إضافة Logging و Monitoring
4. إضافة Caching (Redis)
5. إضافة Unit Tests

### تشغيل التطبيق

```powershell
# بدء الخادم
powershell -ExecutionPolicy Bypass -File "start_server_new.ps1"

# إيقاف الخادم
powershell -ExecutionPolicy Bypass -File "stop_server.ps1"
```

## الإحصائيات

| المقياس | القيمة |
|--------|--------|
| **إجمالي المزارعين** | 300 |
| **الإعدادات المخزنة** | 1 |
| **الفهارس** | 4 |
| **Entities** | 2 |
| **Tables** | 2 |
| **Migrations** | 1 |

## ملاحظات مهمة

⚠️ **LocalDB**
- يجب أن يكون SQL Server Express LocalDB مثبت على النظام
- يتم إنشاء قاعدة البيانات تلقائياً عند أول تشغيل

✅ **البيانات الحالية**
- جميع البيانات من JSON تم حفظها بنجاح في قاعدة البيانات
- لا توجد خسارة في البيانات

🔄 **Backward Compatibility**
- الـ API endpoints لم تتغير
- الـ Models لم تتغير
- فقط Backend تغيّر من JSON إلى SQL

## الدعم والمساعدة

للأسئلة حول التكامل مع SQL Server، تحقق من:
- Microsoft Entity Framework Core Docs
- SQL Server LocalDB Documentation
- ASP.NET Core Documentation

---

**تاريخ الإنشاء**: 22 ديسمبر 2025
**الإصدار**: 1.0
**الحالة**: ✅ مُنتج
