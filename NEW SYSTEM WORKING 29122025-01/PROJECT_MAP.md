# خريطة المشروع (FarmersApp / NEW SYSTEM)

هذا الملف يشرح كيف يتكوّن المشروع وما هو مسار البيانات من الواجهة إلى الـ API ثم إلى قاعدة البيانات.

## 1) نظرة عامة سريعة

- **نوع المشروع**: ASP.NET Core Web API + صفحات HTML ثابتة (Static HTML)
- **المنفذ**: يعمل على `http://0.0.0.0:5000`
- **الواجهة**: ملفات HTML داخل مجلد `Views/` يتم تقديمها مباشرة.
- **الـ API**: مسارات تحت `/api/...`.
- **قاعدة البيانات**: SQL Server عبر Entity Framework Core (EF Core) + Migrations.
- **قراءة الحساسات**: عبر `System.IO.Ports` (COM ports) داخل `SerialPortService`.

## 2) مخطط مكونات (Architecture Map)

```text
                   ┌─────────────────────────────┐
                   │         Browser (UI)         │
                   │  Views/*.html + JavaScript   │
                   └──────────────┬──────────────┘
                                  │
                                  │ HTTP GET pages
                                  ▼
                         ┌─────────────────┐
                         │ PagesController  │
                         │  /, /settings…   │
                         └─────────────────┘
                                  │
                                  │ fetch('/api/...')
                                  ▼
                         ┌─────────────────┐
                         │  ApiController   │
                         │   /api/*         │
                         └───────┬─────────┘
                                 │
     ┌───────────────────────────┼───────────────────────────┐
     │                           │                           │
     ▼                           ▼                           ▼
┌───────────────┐        ┌─────────────────┐         ┌─────────────────┐
│SerialPortService│        │  FarmersService │         │ MilkEntryService │
│ COM sensors     │        │ Farmers CRUD    │         │ Milk Entries     │
└───────┬────────┘        └────────┬────────┘         └────────┬────────┘
        │                           │                           │
        │ (Queues in memory)        │ EF Core                   │ EF Core
        │                           ▼                           ▼
        │                    ┌─────────────────┐        ┌─────────────────┐
        │                    │   AppDbContext   │        │   AppDbContext   │
        │                    │ (EF Core)        │        │ (EF Core)        │
        │                    └────────┬────────┘        └────────┬────────┘
        │                             │                           │
        ▼                             ▼                           ▼
  /api/data etc.                SQL Server DB                SQL Server DB

                 ┌────────────────────┐
                 │   SettingsService   │
                 │ settings (DB+JSON)  │
                 └─────────┬──────────┘
                           │ EF Core
                           ▼
                      SQL Server DB
```

## 3) ملفات ومجلدات مهمة

### 3.1 نقطة التشغيل (Startup)
- **`Program.cs`**
  - تسجيل الخدمات (DI)
  - إعداد EF Core + SQL Server
  - تشغيل `dbContext.Database.Migrate()` عند الإقلاع
  - إعداد Static Files من مجلد `Views/`

### 3.2 Controllers
- **`Controllers/PagesController.cs`**
  - يعرض صفحات HTML من `Views/` لمسارات مثل `/`, `/settings`, `/reports`, `/farmers`...
- **`Controllers/ApiController.cs`**
  - يحتوي جميع Endpoints الخاصة بالـ API تحت `/api`.

### 3.3 Services (Business Logic)
- **`Services/SerialPortService.cs`**
  - الاتصال ب COM (حساس الجودة + حساس الكمية)
  - قراءة البيانات في الخلفية وتخزين آخر القراءات في Queues
- **`Services/SettingsService.cs`**
  - إعدادات النظام من DB أولاً
  - إذا DB فاضية: يحاول القراءة من `settings.json`
- **`Services/FarmersService.cs`**
  - إدارة المزارعين من DB أولاً
  - إذا DB فاضية: يحاول القراءة من `farmers.json`
- **`Services/MilkEntryService.cs`**
  - إدارة إدخالات الحليب
  - حساب السعر بناءً على أسعار الأبقار/الإبل الموجودة في الإعدادات

### 3.4 Data (Database)
- **`Data/AppDbContext.cs`**
  - تعريف الجداول: `Farmers`, `Settings`, `MilkEntries`
  - إعداد القيود والـ indexes
  - Seed لإعدادات ابتدائية (Id=1)
- **`Data/Entities/*`**
  - تعريف Entities (شكل الجداول في EF Core)
- **`Migrations/*`**
  - ملفات ترحيل قاعدة البيانات (إنشاء وتحديث schema)

### 3.5 Models (DTOs)
- **`Models/*`**
  - نماذج تُستخدم في تبادل البيانات عبر API (مثلاً FarmerModel/MilkEntryModel/SettingsModel).

## 4) مسارات الصفحات (Pages)

هذه صفحات HTML يتم تقديمها من `Views/`:

- `/` → `Views/index.html`
- `/settings` → `Views/settings.html`
- `/tools` → `Views/tools.html`
- `/reports` → `Views/reports.html`
- `/farmer_report` → `Views/farmer_report.html`
- `/com_settings` → `Views/com_settings.html`
- `/register` → `Views/register.html`
- `/farmers` → `Views/farmers_management_pro.html`
- `/farmers-management` → `Views/farmers_management.html`

> ملاحظة: هذه المسارات معرفة داخل `PagesController`.

## 5) خريطة الـ API (مختصر)

كلها تحت `/api`:

### 5.1 Backup
- `GET /api/backup/all` تنزيل نسخة احتياطية ZIP (farmers + milk entries + settings)

### 5.2 COM / Sensors
- `POST /api/connect` اتصال حساس الجودة
- `POST /api/disconnect` فصل حساس الجودة
- `POST /api/connect_quantity` اتصال حساس الكمية
- `POST /api/disconnect_quantity` فصل حساس الكمية
- `GET /api/ports` قائمة COM Ports المتاحة
- `GET /api/data` قراءات الحساسات الحالية (من الذاكرة/Queue)
- `GET /api/status` حالة الاتصال

### 5.3 Settings
- `GET /api/settings` جلب الإعدادات
- `POST /api/settings` حفظ الإعدادات

### 5.4 Farmers
- `GET /api/farmers` جلب المزارعين
- `POST /api/farmers` إضافة مزارع
- `PUT /api/farmers/{code}` تحديث مزارع
- `DELETE /api/farmers/{code}` حذف مزارع
- `POST /api/farmers/restore` استعادة من Backup

### 5.5 Milk Entries
- `POST /api/milk-entries` إضافة إدخال
- `GET /api/milk-entries` عرض كل الإدخالات
- `GET /api/milk-entries/farmer/{farmerCode}` إدخالات مزارع
- `GET /api/milk-entries/range?startDate=...&endDate=...` نطاق تاريخ
- `GET /api/milk-entries/{id}` إدخال محدد
- `PUT /api/milk-entries/{id}` تحديث
- `DELETE /api/milk-entries/{id}` حذف
- `PATCH /api/milk-entries/{id}/confirm` اعتماد
- `PATCH /api/milk-entries/{id}/reject?reason=...` رفض

### 5.6 Statistics
- `GET /api/milk-statistics/daily?date=...`
- `GET /api/milk-statistics/range?startDate=...&endDate=...`
- `GET /api/milk-statistics/pending-count`

## 6) مسار البيانات (Data Flow) — مثالين

### مثال A: فتح الصفحة الرئيسية وعرض البيانات
```text
GET /  -> PagesController -> Views/index.html
index.html -> fetch('/api/settings') -> ApiController -> SettingsService -> AppDbContext -> SQL Server
index.html -> fetch('/api/data')     -> ApiController -> SerialPortService (Queues)
```

### مثال B: إضافة إدخال حليب
```text
index.html -> POST /api/milk-entries
ApiController -> MilkEntryService -> AppDbContext -> SQL Server
MilkEntryService يحسب السعر من Settings (MilkPriceCow/MilkPriceCamel)
```

## 7) تشغيل سريع (ملخص)

- تشغيل التطبيق: راجع ملفات `run.ps1` أو `run.bat` أو `start_server_new.ps1` حسب بيئتك.
- قاعدة البيانات: Connection String داخل `appsettings.json`.
- عند الإقلاع يتم تطبيق Migrations تلقائياً.
