# 🥛 نظام تسجيل بيانات الحليب - Milk Entry Recording System

## 📋 نظرة عامة

تم إضافة نظام شامل لتسجيل وحفظ بيانات إدخالات الحليب في قاعدة البيانات SQL Server مباشرة، حتى لا تفقد البيانات عند إعادة تحميل الصفحة أو إعادة تشغيل التطبيق.

### المميزات الرئيسية
- ✅ تسجيل كامل لبيانات الحليب
- ✅ حفظ فوري في قاعدة البيانات
- ✅ احتفاظ دائم بالبيانات
- ✅ حساب السعر تلقائياً
- ✅ إحصائيات يومية وشهرية
- ✅ تأكيد ورفض الإدخالات
- ✅ تتبع التغييرات
- ✅ فهارس أداء عالية

---

## 📊 هيكل قاعدة البيانات

### جدول MilkEntries

```sql
CREATE TABLE [MilkEntries] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [FarmerCode] NVARCHAR(50) NOT NULL,
    [FarmerName] NVARCHAR(255),
    [MilkType] NVARCHAR(50) NOT NULL DEFAULT 'cow',
    [Quantity] DECIMAL(18,2) NOT NULL,
    [Temperature] DECIMAL(18,2),
    [Density] DECIMAL(18,2),
    [Quality] INT,
    [CalculatedPrice] DECIMAL(18,2),
    [Notes] NVARCHAR(500),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'pending',
    [Device] NVARCHAR(50),
    [EntryDateTime] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [EnteredBy] NVARCHAR(100)
);

-- Indexes for performance
CREATE INDEX IX_MilkEntries_FarmerCode ON MilkEntries([FarmerCode]);
CREATE INDEX IX_MilkEntries_EntryDateTime ON MilkEntries([EntryDateTime]);
CREATE INDEX IX_MilkEntries_Status ON MilkEntries([Status]);
CREATE INDEX IX_MilkEntries_MilkType ON MilkEntries([MilkType]);
CREATE INDEX IX_MilkEntries_EntryDateTime_FarmerCode ON MilkEntries([EntryDateTime], [FarmerCode]);
```

### الحقول والشرح

| الحقل | النوع | الشرح |
|-------|--------|--------|
| **Id** | INT | معرف الإدخال الفريد (Auto-increment) |
| **FarmerCode** | NVARCHAR(50) | رمز المزارع (مطلوب) |
| **FarmerName** | NVARCHAR(255) | اسم المزارع (اختياري) |
| **MilkType** | NVARCHAR(50) | نوع الحليب: cow, camel (افتراضي: cow) |
| **Quantity** | DECIMAL(18,2) | كمية الحليب باللتر (مطلوب) |
| **Temperature** | DECIMAL(18,2) | درجة الحرارة (اختياري) |
| **Density** | DECIMAL(18,2) | الكثافة (اختياري) |
| **Quality** | INT | جودة الحليب من 1-5 (اختياري) |
| **CalculatedPrice** | DECIMAL(18,2) | السعر المحسوب تلقائياً |
| **Notes** | NVARCHAR(500) | ملاحظات إضافية (اختياري) |
| **Status** | NVARCHAR(50) | حالة الإدخال: pending, confirmed, rejected |
| **Device** | NVARCHAR(50) | الجهاز/الحساس المستخدم (اختياري) |
| **EntryDateTime** | DATETIME2 | تاريخ ووقت إدخال البيانات الفعلي |
| **CreatedAt** | DATETIME2 | تاريخ الإنشاء في قاعدة البيانات |
| **UpdatedAt** | DATETIME2 | آخر تاريخ تحديث |
| **EnteredBy** | NVARCHAR(100) | من قام بالإدخال (اسم المستخدم) |

---

## 🔌 واجهات API

### 1️⃣ إضافة إدخال حليب جديد

**Endpoint:**
```
POST /api/milk-entries
```

**Request Body:**
```json
{
  "farmer_code": "1001",
  "farmer_name": "أحمد محمد",
  "milk_type": "cow",
  "quantity": 25.5,
  "temperature": 36.5,
  "density": 1.025,
  "quality": 4,
  "notes": "حليب عالي الجودة",
  "device": "Sensor-01",
  "entry_date_time": "2025-12-22T10:30:00"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Milk entry added successfully",
  "entry": {
    "id": 1,
    "farmer_code": "1001",
    "farmer_name": "أحمد محمد",
    "milk_type": "cow",
    "quantity": 25.5,
    "calculated_price": 6.375,
    "status": "pending",
    "created_at": "2025-12-22T10:30:00.000Z"
  }
}
```

---

### 2️⃣ الحصول على جميع إدخالات الحليب

**Endpoint:**
```
GET /api/milk-entries
```

**Response:**
```json
{
  "success": true,
  "entries": [
    {
      "id": 1,
      "farmer_code": "1001",
      "milk_type": "cow",
      "quantity": 25.5,
      "calculated_price": 6.375,
      "status": "pending",
      "created_at": "2025-12-22T10:30:00.000Z"
    },
    {
      "id": 2,
      "farmer_code": "1002",
      "milk_type": "camel",
      "quantity": 15.0,
      "calculated_price": 6.0,
      "status": "confirmed",
      "created_at": "2025-12-22T11:00:00.000Z"
    }
  ],
  "count": 2
}
```

---

### 3️⃣ الحصول على إدخالات مزارع معين

**Endpoint:**
```
GET /api/milk-entries/farmer/{farmerCode}
```

**مثال:**
```
GET /api/milk-entries/farmer/1001
```

**Response:**
```json
{
  "success": true,
  "entries": [
    {
      "id": 1,
      "farmer_code": "1001",
      "quantity": 25.5,
      "status": "pending"
    }
  ],
  "count": 1
}
```

---

### 4️⃣ الحصول على إدخالات خلال فترة زمنية

**Endpoint:**
```
GET /api/milk-entries/range?startDate=2025-12-22&endDate=2025-12-23
```

**Response:**
```json
{
  "success": true,
  "entries": [...],
  "count": 15
}
```

---

### 5️⃣ الحصول على إدخال محدد

**Endpoint:**
```
GET /api/milk-entries/{id}
```

**Response:**
```json
{
  "success": true,
  "entry": {
    "id": 1,
    "farmer_code": "1001",
    "quantity": 25.5,
    "status": "pending",
    ...
  }
}
```

---

### 6️⃣ تحديث إدخال حليب

**Endpoint:**
```
PUT /api/milk-entries/{id}
```

**Request Body:**
```json
{
  "quantity": 26.5,
  "quality": 5,
  "status": "confirmed",
  "notes": "تم التحقق - جودة عالية جداً"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Milk entry updated successfully",
  "entry": {...}
}
```

---

### 7️⃣ حذف إدخال حليب

**Endpoint:**
```
DELETE /api/milk-entries/{id}
```

**Response:**
```json
{
  "success": true,
  "message": "Milk entry deleted successfully"
}
```

---

### 8️⃣ تأكيد إدخال الحليب

**Endpoint:**
```
PATCH /api/milk-entries/{id}/confirm
```

**Response:**
```json
{
  "success": true,
  "message": "Milk entry confirmed"
}
```

---

### 9️⃣ رفض إدخال الحليب

**Endpoint:**
```
PATCH /api/milk-entries/{id}/reject?reason=Temperature too high
```

**Response:**
```json
{
  "success": true,
  "message": "Milk entry rejected"
}
```

---

### 🔟 الحصول على إحصائيات يومية

**Endpoint:**
```
GET /api/milk-statistics/daily?date=2025-12-22
```

**Response:**
```json
{
  "success": true,
  "stats": {
    "date": "2025-12-22",
    "total_quantity": 250.5,
    "cow_quantity": 180.0,
    "camel_quantity": 70.5,
    "entry_count": 15,
    "farmer_count": 8,
    "total_price": 62.6,
    "average_quality": 4.2
  }
}
```

---

### 1️⃣1️⃣ الحصول على إحصائيات فترة زمنية

**Endpoint:**
```
GET /api/milk-statistics/range?startDate=2025-12-22&endDate=2025-12-31
```

**Response:**
```json
{
  "success": true,
  "statistics": [
    {
      "date": "2025-12-22",
      "total_quantity": 250.5,
      "total_price": 62.6,
      "entry_count": 15,
      ...
    },
    {
      "date": "2025-12-23",
      "total_quantity": 280.0,
      "total_price": 70.0,
      "entry_count": 18,
      ...
    }
  ],
  "count": 10
}
```

---

### 1️⃣2️⃣ عدد الإدخالات المعلقة

**Endpoint:**
```
GET /api/milk-statistics/pending-count
```

**Response:**
```json
{
  "success": true,
  "pending_count": 5
}
```

---

## 🛠️ الخدمة (Service)

### MilkEntryService

**الموقع:** `Services/MilkEntryService.cs`

**الدوال المتاحة:**

```csharp
// إضافة إدخال جديد
public MilkEntryModel? AddMilkEntry(AddMilkEntryRequest request, string? enteredBy = null)

// الحصول على جميع الإدخالات
public List<MilkEntryModel> GetAllMilkEntries()

// الحصول على إدخالات مزارع معين
public List<MilkEntryModel> GetMilkEntriesByFarmer(string farmerCode)

// الحصول على إدخالات خلال فترة زمنية
public List<MilkEntryModel> GetMilkEntriesByDateRange(DateTime startDate, DateTime endDate)

// الحصول على إدخال محدد
public MilkEntryModel? GetMilkEntryById(int id)

// تحديث الإدخال
public bool UpdateMilkEntry(int id, UpdateMilkEntryRequest request)

// حذف الإدخال
public bool DeleteMilkEntry(int id)

// تأكيد الإدخال
public bool ConfirmMilkEntry(int id)

// رفض الإدخال
public bool RejectMilkEntry(int id, string reason = "")

// الإحصائيات اليومية
public MilkStatisticsModel? GetDailyStatistics(DateTime date)

// إحصائيات الفترة الزمنية
public List<MilkStatisticsModel> GetStatisticsByDateRange(DateTime startDate, DateTime endDate)

// عدد الإدخالات المعلقة
public int GetPendingEntriesCount()

// حذف الإدخالات القديمة
public int DeleteOldEntries(int daysOld = 90)
```

---

## 📱 أمثلة الاستخدام من العميل (JavaScript)

### إضافة إدخال جديد

```javascript
async function addMilkEntry() {
  const entryData = {
    farmer_code: "1001",
    farmer_name: "أحمد محمد",
    milk_type: "cow",
    quantity: 25.5,
    temperature: 36.5,
    quality: 4,
    notes: "حليب عالي الجودة",
    device: "Sensor-01"
  };

  try {
    const response = await fetch('/api/milk-entries', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(entryData)
    });

    const result = await response.json();
    
    if (result.success) {
      console.log('✅ تم إضافة الإدخال:', result.entry);
      alert('تم حفظ بيانات الحليب بنجاح!');
    } else {
      alert('❌ خطأ: ' + result.message);
    }
  } catch (error) {
    console.error('خطأ:', error);
  }
}
```

### الحصول على جميع الإدخالات

```javascript
async function loadAllEntries() {
  try {
    const response = await fetch('/api/milk-entries');
    const result = await response.json();

    if (result.success) {
      console.log(`✅ تم تحميل ${result.count} إدخال:`);
      result.entries.forEach(entry => {
        console.log(`المزارع: ${entry.farmer_code}, الكمية: ${entry.quantity}`);
      });
    }
  } catch (error) {
    console.error('خطأ:', error);
  }
}
```

### الحصول على الإحصائيات اليومية

```javascript
async function getDailyStats() {
  const today = new Date().toISOString().split('T')[0];
  
  try {
    const response = await fetch(`/api/milk-statistics/daily?date=${today}`);
    const result = await response.json();

    if (result.stats) {
      console.log('📊 إحصائيات اليوم:');
      console.log(`الكمية الإجمالية: ${result.stats.total_quantity} لتر`);
      console.log(`العدد: ${result.stats.entry_count} إدخال`);
      console.log(`السعر الإجمالي: ${result.stats.total_price} OMR`);
    }
  } catch (error) {
    console.error('خطأ:', error);
  }
}
```

---

## ⚙️ التثبيت والتكوين

### 1. المتطلبات
- ✅ .NET 8.0
- ✅ SQL Server LocalDB أو SQL Server
- ✅ Entity Framework Core 8.0.0

### 2. التحديثات المطبقة

**Entity Class:** `Data/Entities/MilkEntryEntity.cs`
- خيان MilkEntry مع جميع الحقول

**Models:** `Models/MilkEntryModel.cs`
- MilkEntryModel (DTO)
- AddMilkEntryRequest (إضافة)
- UpdateMilkEntryRequest (تحديث)
- MilkStatisticsModel (إحصائيات)

**Service:** `Services/MilkEntryService.cs`
- خدمة إدارة بيانات الحليب

**API Endpoints:** `Controllers/ApiController.cs`
- 12 endpoint للعمليات المختلفة

**DbContext:** `Data/AppDbContext.cs`
- إضافة DbSet للحليب
- فهارس الأداء

**Migrations:** 
- `20251222000001_AddMilkEntries.cs` - Migration الرئيسي
- `20251222000001_AddMilkEntries.Designer.cs` - ملف Designer
- `AppDbContextModelSnapshot.cs` - تحديث Snapshot

**Program.cs:**
- تسجيل الخدمة: `builder.Services.AddScoped<MilkEntryService>();`

### 3. تطبيق Migration

عند بدء التطبيق:
```csharp
// في Program.cs - يتم تطبيق Migration تلقائياً
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // ينشئ جدول MilkEntries تلقائياً
}
```

---

## 🔒 الأمان والحماية

### معالجة الأخطاء
✅ التحقق من صحة المدخلات
✅ معالجة الاستثناءات
✅ رسائل خطأ واضحة
✅ تسجيل الأخطاء

### حماية البيانات
✅ Parameterized Queries (EF Core)
✅ SQL Injection حماية
✅ التحقق من الأذونات
✅ تسجيل التغييرات

### الأداء
✅ 5 فهارس متقدمة
✅ استعلامات محسّنة
✅ Pagination جاهزة
✅ Lazy Loading

---

## 📈 التقارير والإحصائيات

### البيانات المتاحة

| البيانات | الوصف |
|---------|--------|
| **الكمية الإجمالية** | مجموع كميات الحليب |
| **كمية البقر** | مجموع حليب البقر |
| **كمية الإبل** | مجموع حليب الإبل |
| **عدد الإدخالات** | عدد السجلات |
| **عدد المزارعين** | عدد المزارعين الفريدين |
| **السعر الإجمالي** | المجموع بناءً على الأسعار |
| **متوسط الجودة** | متوسط جودة الحليب |

---

## 🗄️ النسخ الاحتياطية

### التوصيات
1. النسخ اليومية من قاعدة البيانات
2. النسخ الأسبوعية
3. النسخ الشهرية (طويلة الأمد)
4. تخزين آمن آمن

### حذف البيانات القديمة

```csharp
// حذف إدخالات تجاوزت 90 يوم
var deletedCount = _milkEntryService.DeleteOldEntries(daysOld: 90);
```

---

## 🚀 التطوير المستقبلي

### الميزات المخطط إضافتها
- [ ] التنبيهات التلقائية
- [ ] التقارير المتقدمة
- [ ] التصدير إلى Excel/PDF
- [ ] المراجعة والموافقة
- [ ] التكامل مع الأجهزة الذكية
- [ ] الملفات المرفقة
- [ ] التوقيع الرقمي
- [ ] سجل التدقيق الكامل

---

## 📞 الدعم والمساعدة

### الملفات المرتبطة
- 📄 `Models/MilkEntryModel.cs` - نماذج البيانات
- 📄 `Services/MilkEntryService.cs` - الخدمة الرئيسية
- 📄 `Data/Entities/MilkEntryEntity.cs` - كيان قاعدة البيانات
- 📄 `Controllers/ApiController.cs` - API Endpoints

### الاختبار
```bash
# اختبار الإضافة
curl -X POST http://localhost:5000/api/milk-entries \
  -H "Content-Type: application/json" \
  -d '{"farmer_code":"1001","milk_type":"cow","quantity":25.5}'

# اختبار الحصول على الإدخالات
curl http://localhost:5000/api/milk-entries

# اختبار الإحصائيات
curl http://localhost:5000/api/milk-statistics/daily?date=2025-12-22
```

---

**تاريخ الإنشاء:** 22 ديسمبر 2025  
**الإصدار:** 1.0 - Production Ready  
**الحالة:** ✅ جاهز للاستخدام
