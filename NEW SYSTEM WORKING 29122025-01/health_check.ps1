#!/usr/bin/env pwsh
# Farmers App - System Health Check
# الإصدار: 1.0
# فحص شامل لجاهزية النظام

$script:TotalChecks = 0
$script:PassedChecks = 0
$script:FailedChecks = 0

function Test-Component {
    param(
        [string]$Name,
        [scriptblock]$TestBlock,
        [string]$Category = "General"
    )
    
    $script:TotalChecks++
    Write-Host "  ⏳ " -ForegroundColor Cyan -NoNewline
    Write-Host "فحص $Name..." -NoNewline
    
    try {
        $result = & $TestBlock
        if ($result) {
            Write-Host " ✅" -ForegroundColor Green
            $script:PassedChecks++
            return $true
        } else {
            Write-Host " ❌" -ForegroundColor Red
            $script:FailedChecks++
            return $false
        }
    } catch {
        Write-Host " ❌ ($_)" -ForegroundColor Red
        $script:FailedChecks++
        return $false
    }
}

# الرأس
Clear-Host
Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║" -ForegroundColor Magenta -NoNewline; Write-Host "         Farmers App - System Health Check" -ForegroundColor Magenta -NoNewline; Write-Host "                 ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Write-Host "`nفحص التاريخ والوقت: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" -ForegroundColor Gray

# 1. فحوصات .NET
Write-Host "🔹 فحوصات .NET Framework" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component ".NET SDK المثبت" {
    $dotnet = dotnet --version 2>$null
    if ($dotnet) {
        Write-Host " (النسخة: $dotnet)" -ForegroundColor Gray -NoNewline
        return $true
    }
    return $false
}

Test-Component "قاعدة العمل الحالية" {
    $cwd = (Get-Location).Path
    Write-Host " ($cwd)" -ForegroundColor Gray -NoNewline
    return $true
}

# 2. فحوصات ملفات المشروع
Write-Host "`n🔹 فحوصات ملفات المشروع" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "ملف المشروع (FarmersApp.csproj)" {
    Test-Path ".\FarmersApp.csproj"
}

Test-Component "ملف Program.cs" {
    Test-Path ".\Program.cs"
}

Test-Component "مجلد Controllers" {
    Test-Path ".\Controllers"
}

Test-Component "مجلد Services" {
    Test-Path ".\Services"
}

Test-Component "مجلد Data" {
    Test-Path ".\Data"
}

Test-Component "مجلد Migrations" {
    Test-Path ".\Migrations"
}

Test-Component "مجلد Models" {
    Test-Path ".\Models"
}

Test-Component "مجلد templates" {
    Test-Path ".\templates"
}

Test-Component "ملف appsettings.json" {
    Test-Path ".\appsettings.json"
}

Test-Component "ملف farmers.json (البيانات)" {
    Test-Path ".\farmers.json"
}

# 3. فحوصات SQL Server
Write-Host "`n🔹 فحوصات SQL Server" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "SQL Server LocalDB موجود" {
    $sqlPath = "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe"
    Test-Path $sqlPath
}

Test-Component "خدمة MSSQLLocalDB" {
    try {
        $service = Get-Service MSSQLLocalDB -ErrorAction SilentlyContinue
        if ($service) {
            Write-Host " (الحالة: $($service.Status))" -ForegroundColor Gray -NoNewline
            return $true
        }
        return $false
    } catch {
        return $false
    }
}

Test-Component "استجابة SqlLocalDB" {
    try {
        $result = & "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe" info mssqllocaldb 2>$null
        return $result -ne $null
    } catch {
        return $false
    }
}

# 4. فحوصات الاتصال
Write-Host "`n🔹 فحوصات الاتصال والشبكة" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "الاتصال بالإنترنت" {
    try {
        $result = Test-NetConnection 8.8.8.8 -Port 53 -WarningAction SilentlyContinue
        return $result.PingSucceeded
    } catch {
        return $false
    }
}

Test-Component "المنفذ 5000 متاح" {
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.ConnectAsync("localhost", 5000).Wait(1000)
        $connection.Close()
        return $false # إذا كان الاتصال ناجحاً، يعني يوجد تطبيق يعمل بالفعل
    } catch {
        return $true # المنفذ متاح (لا يوجد تطبيق)
    }
}

# 5. فحوصات الملفات الرئيسية
Write-Host "`n🔹 فحوصات الملفات الرئيسية" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "AppDbContext.cs موجود" {
    Test-Path ".\Data\AppDbContext.cs"
}

Test-Component "FarmerEntity.cs موجود" {
    Test-Path ".\Data\Entities\FarmerEntity.cs"
}

Test-Component "SettingsEntity.cs موجود" {
    Test-Path ".\Data\Entities\SettingsEntity.cs"
}

Test-Component "FarmersService.cs موجود" {
    Test-Path ".\Services\FarmersService.cs"
}

Test-Component "SettingsService.cs موجود" {
    Test-Path ".\Services\SettingsService.cs"
}

Test-Component "ApiController.cs موجود" {
    Test-Path ".\Controllers\ApiController.cs"
}

Test-Component "PagesController.cs موجود" {
    Test-Path ".\Controllers\PagesController.cs"
}

# 6. فحوصات صفحات HTML
Write-Host "`n🔹 فحوصات صفحات HTML" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

$htmlFiles = @("index.html", "farmers_management_pro.html", "settings.html", "com_settings.html")
foreach ($file in $htmlFiles) {
    Test-Component "الملف $file" {
        Test-Path ".\templates\$file"
    }
}

# 7. فحوصات الملفات الوثائقية
Write-Host "`n🔹 فحوصات الملفات الوثائقية" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "SQL_SERVER_DOCUMENTATION.md" {
    Test-Path ".\SQL_SERVER_DOCUMENTATION.md"
}

Test-Component "SQL_SERVER_INTEGRATION_SUCCESS.md" {
    Test-Path ".\SQL_SERVER_INTEGRATION_SUCCESS.md"
}

Test-Component "STARTUP_GUIDE.md" {
    Test-Path ".\STARTUP_GUIDE.md"
}

# 8. فحوصات الملفات الاختيارية
Write-Host "`n🔹 فحوصات إضافية" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

Test-Component "سكريبت التشغيل (launch_app.ps1)" {
    Test-Path ".\launch_app.ps1"
}

Test-Component "ملف البيانات الاحتياطية (farmers_backup)" {
    $backups = Get-Item ".\farmers_backup*.json" -ErrorAction SilentlyContinue
    return $backups.Count -gt 0
}

# 9. فحوصات الذاكرة والموارد
Write-Host "`n🔹 فحوصات الموارد" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# الذاكرة
$ram = Get-CimInstance Win32_OperatingSystem
$totalMem = [math]::Round($ram.TotalVisibleMemorySize / 1MB, 2)
$freeMem = [math]::Round($ram.FreePhysicalMemory / 1MB, 2)
$usedPercent = [math]::Round(($totalMem - $freeMem) / $totalMem * 100, 2)

Write-Host "  📊 " -ForegroundColor Cyan -NoNewline
Write-Host "الذاكرة: $freeMem MB / $totalMem MB متاحة ($usedPercent% مستخدمة)"

# المساحة الخالية
$disk = Get-PSDrive C
$freeSpace = [math]::Round($disk.Free / 1GB, 2)
$totalSpace = [math]::Round($disk.Used / 1GB + $disk.Free / 1GB, 2)

Write-Host "  💾 " -ForegroundColor Cyan -NoNewline
Write-Host "محرك الأقراص: $freeSpace GB / $totalSpace GB متاح"

# النتيجة النهائية
Write-Host "`n╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║" -ForegroundColor Magenta -NoNewline; Write-Host " النتائج النهائية" -ForegroundColor Magenta -NoNewline; Write-Host (" " * 47) "║" -ForegroundColor Magenta

$percentage = [math]::Round(($script:PassedChecks / $script:TotalChecks) * 100)
$status = if ($percentage -eq 100) { "✅ جاهز تماماً" } elseif ($percentage -ge 80) { "⚠️  جاهز مع تحفظات" } else { "❌ يحتاج إلى تعديل" }

Write-Host "║" -ForegroundColor Magenta -NoNewline
Write-Host "  النتيجة: " -ForegroundColor White -NoNewline
Write-Host "$script:PassedChecks / $script:TotalChecks فحص نجح ($percentage%)" -ForegroundColor Green -NoNewline
Write-Host (" " * (32 - "$percentage%".Length)) "║" -ForegroundColor Magenta

Write-Host "║" -ForegroundColor Magenta -NoNewline
Write-Host "  الحالة: $status" -ForegroundColor White -NoNewline
Write-Host (" " * 48) "║" -ForegroundColor Magenta

Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

# الإجراء المقترح
Write-Host "`n🚀 الخطوات التالية:" -ForegroundColor Green
if ($percentage -eq 100) {
    Write-Host "  ✅ يمكنك بدء التطبيق الآن!"
    Write-Host "     اكتب: .\launch_app.ps1"
} else {
    Write-Host "  ⚠️  يوجد بعض المشاكل التي يجب حلها أولاً"
    Write-Host "     تحقق من الفحوصات التي أظهرت ❌"
}

Write-Host "`n"
