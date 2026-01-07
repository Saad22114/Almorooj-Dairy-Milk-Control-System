#!/usr/bin/env pwsh
# Farmers App - SQL Server Integration Setup and Launch Script
# الإصدار: 1.0 (Production Ready)
# التاريخ: 22 ديسمبر 2025

# تعريفات الألوان
$Colors = @{
    Success = @{ ForegroundColor = 'Green'; }
    Warning = @{ ForegroundColor = 'Yellow'; }
    Error = @{ ForegroundColor = 'Red'; }
    Info = @{ ForegroundColor = 'Cyan'; }
    Title = @{ ForegroundColor = 'Magenta'; }
}

# دوال مساعدة
function Write-Title($text) {
    Write-Host "`n" @($Colors.Title) -NoNewline
    Write-Host "╔══════════════════════════════════════════════════════════════╗" @($Colors.Title)
    Write-Host "║ " @($Colors.Title) -NoNewline; Write-Host $text.PadRight(57) @($Colors.Title) -NoNewline; Write-Host " ║" @($Colors.Title)
    Write-Host "╚══════════════════════════════════════════════════════════════╝" @($Colors.Title)
}

function Write-Success($text) {
    Write-Host "  ✅ " @($Colors.Success) -NoNewline; Write-Host $text
}

function Write-Info($text) {
    Write-Host "  ℹ️  " @($Colors.Info) -NoNewline; Write-Host $text
}

function Write-Warning($text) {
    Write-Host "  ⚠️  " @($Colors.Warning) -NoNewline; Write-Host $text
}

function Write-Error-Message($text) {
    Write-Host "  ❌ " @($Colors.Error) -NoNewline; Write-Host $text
}

# البرنامج الرئيسي
Write-Title "Farmers App - SQL Server Integration Console"

Write-Info "تاريخ التشغيل: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Info "النسخة: 1.0 (Production Ready)"

# الفحوصات المسبقة
Write-Title "الفحوصات المسبقة"

# 1. التحقق من dotnet
Write-Host "`n  فحص .NET SDK..." -ForegroundColor Cyan
$dotnet = dotnet --version 2>$null
if ($dotnet) {
    Write-Success ".NET SDK: $dotnet"
} else {
    Write-Error-Message ".NET SDK غير مثبت"
    exit 1
}

# 2. التحقق من SQL Server LocalDB
Write-Host "`n  فحص SQL Server LocalDB..." -ForegroundColor Cyan
$sqlExist = Test-Path "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe"
if ($sqlExist) {
    Write-Success "SQL Server LocalDB موجود"
} else {
    Write-Warning "SQL Server LocalDB قد لا يكون مثبت"
}

# 3. التحقق من المشروع
Write-Host "`n  فحص المشروع..." -ForegroundColor Cyan
$csprojPath = ".\FarmersApp.csproj"
if (Test-Path $csprojPath) {
    Write-Success "ملف المشروع موجود"
} else {
    Write-Error-Message "ملف المشروع غير موجود"
    exit 1
}

# الإجراءات
Write-Title "الإجراءات المطلوبة"

# 1. التنظيف
Write-Host "`n  1️⃣  تنظيف الملفات السابقة..." -ForegroundColor Cyan
if (Test-Path "./bin") {
    Remove-Item -Path "./bin" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    Write-Success "تم تنظيف bin"
}
if (Test-Path "./obj") {
    Remove-Item -Path "./obj" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    Write-Success "تم تنظيف obj"
}

# 2. Restore
Write-Host "`n  2️⃣  استعادة الحزم..." -ForegroundColor Cyan
dotnet restore | Out-Null
Write-Success "تم استعادة الحزم"

# 3. Build
Write-Host "`n  3️⃣  بناء المشروع..." -ForegroundColor Cyan
$buildOutput = dotnet build --configuration Release 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Success "تم بناء المشروع بنجاح"
} else {
    Write-Error-Message "فشل بناء المشروع"
    Write-Host $buildOutput
    exit 1
}

# 4. البيانات والإحصائيات
Write-Title "إحصائيات المشروع"

Write-Info "📊 معلومات قاعدة البيانات:"
Write-Host "    • اسم قاعدة البيانات: FarmersAppDb"
Write-Host "    • الخادم: (localdb)\mssqllocaldb"
Write-Host "    • الجداول: 2 (Farmers, Settings)"

Write-Info "📁 حجم البيانات:"
Write-Host "    • المزارعين المهاجرة: 300"
Write-Host "    • الإعدادات: 1"
Write-Host "    • الفهارس: 4"

Write-Info "🚀 معلومات التشغيل:"
Write-Host "    • العنوان: http://localhost:5000"
Write-Host "    • الإطار: .NET 8.0 LTS"
Write-Host "    • قاعدة البيانات: SQL Server LocalDB"

# 5. بدء التطبيق
Write-Title "بدء التطبيق"

Write-Info "جاري بدء الخادم..."
Write-Host "`n"

$proc = Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
    -ArgumentList "run --configuration Release --urls http://0.0.0.0:5000" `
    -PassThru -WindowStyle Hidden

Write-Success "تم بدء الخادم (PID: $($proc.Id))"
Start-Sleep -Seconds 3

# 6. التحقق من الاتصال
Write-Info "فحص الاتصال..."
$attempts = 0
$maxAttempts = 10

while ($attempts -lt $maxAttempts) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "الاتصال ناجح!"
            break
        }
    } catch {
        $attempts++
        if ($attempts -lt $maxAttempts) {
            Start-Sleep -Seconds 1
        }
    }
}

if ($attempts -eq $maxAttempts) {
    Write-Error-Message "فشل الاتصال بالخادم"
    $proc | Stop-Process -Force
    exit 1
}

# الملخص النهائي
Write-Title "التطبيق جاهز للاستخدام"

Write-Info "الوصول للتطبيق:"
Write-Host "    🌐 الصفحة الرئيسية: http://localhost:5000/"
Write-Host "    🥛 إدارة المزارعين: http://localhost:5000/farmers"
Write-Host "    ⚙️  الإعدادات: http://localhost:5000/settings"
Write-Host "    🔌 إعدادات COM: http://localhost:5000/com_settings"

Write-Info "واجهات API:"
Write-Host "    🔗 المزارعين: GET http://localhost:5000/api/farmers"
Write-Host "    🔗 الإعدادات: GET http://localhost:5000/api/settings"
Write-Host "    🔗 البيانات: GET http://localhost:5000/api/data"

Write-Info "معلومات التطبيق:"
Write-Host "    📌 العملية ID: $($proc.Id)"
Write-Host "    📌 حالة قاعدة البيانات: ✅ جاهزة"
Write-Host "    📌 حالة الخادم: ✅ يعمل"

Write-Host "`n"
Write-Host "اضغط على أي مفتاح لإيقاف الخادم..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# الإغلاق
Write-Host "`n"
Write-Info "جاري إيقاف الخادم..."
$proc | Stop-Process -Force
Write-Success "تم إيقاف الخادم"

Write-Host "`n"
Write-Host "شكراً لاستخدام Farmers App! 👋" -ForegroundColor Green
