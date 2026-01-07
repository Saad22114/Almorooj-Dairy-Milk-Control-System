# 🚀 QUICK START GUIDE - Database Implementation

## ⚡ 5-Minute Setup

### Step 1: Create Database
```sql
USE [master];
IF DB_ID('MilkCollectionSystemDb') IS NOT NULL
    DROP DATABASE [MilkCollectionSystemDb];
CREATE DATABASE [MilkCollectionSystemDb];
USE [MilkCollectionSystemDb];
```

### Step 2: Run Full Schema
```
1. Open SQL Server Management Studio (SSMS)
2. Open file: Database_Schema_Production.sql
3. Execute (F5)
4. Wait for completion (~2-3 seconds)
✅ All tables, indexes, views, and procedures created automatically
```

### Step 3: Verify Installation
```sql
-- Check tables
SELECT name FROM sys.tables;
-- Expected: Farmers, MilkEntries, DailyCollectionSummary, etc.

-- Check indexes
SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('Farmers');

-- Check stored procedures
SELECT name FROM sys.procedures;

-- View sample data (quality thresholds)
SELECT * FROM [dbo].[QualityThresholds];
```

### Step 4: Connect from C#
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MilkCollectionSystemDb;Trusted_Connection=true;"
  }
}

// Program.cs
builder.Services.AddDbContext<MilkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
```

---

## 📊 Table Quick Reference

### Farmers Table
```sql
-- Insert a farmer
INSERT INTO [dbo].[Farmers] (
    [FarmerCode], [FirstName], [LastName], [PhoneNumber], 
    [FarmType], [RegistrationArea], [Status]
)
VALUES ('F001', 'Ahmed', 'Al-Mansouri', '+968-1234567', 'COW', 'Muscat', 'ACTIVE');

-- Get farmer
SELECT * FROM [dbo].[Farmers] WHERE [FarmerCode] = 'F001';
```

### Milk Entries Table
```sql
-- Insert milk entry (using stored procedure - recommended)
DECLARE @MilkEntryID BIGINT;
EXEC [dbo].[sp_InsertMilkEntry]
    @FarmerID = 1,
    @EntryDate = '2025-12-22',
    @EntryTime = '08:30:00',
    @MilkType = 'COW',
    @QuantityLiters = 50.25,
    @UnitPrice = 2.5,
    @Temperature = 5.2,
    @QualityScore = 4,
    @OutputMilkEntryID = @MilkEntryID OUTPUT;
SELECT @MilkEntryID;

-- Get entries for farmer
SELECT * FROM [dbo].[vw_RecentMilkEntriesWithDetails] 
WHERE [FarmerID] = 1;
```

---

## 🔍 Common Queries

### Dashboard Queries

**1. Today's Collection Summary**
```sql
SELECT 
    CAST(me.[EntryDate] AS DATE) AS [Date],
    me.[MilkType],
    COUNT(*) AS [EntryCount],
    SUM(me.[QuantityLiters]) AS [TotalQuantity],
    AVG(me.[QualityScore]) AS [AvgQuality],
    SUM(me.[TotalPrice]) AS [TotalAmount]
FROM [dbo].[MilkEntries] me
WHERE me.[EntryDate] = CAST(GETDATE() AS DATE) AND me.[IsDeleted] = 0
GROUP BY me.[EntryDate], me.[MilkType];
```

**2. Farmer Daily Stats**
```sql
SELECT 
    f.[FarmerCode],
    f.[FullName],
    COUNT(me.[MilkEntryID]) AS [TodayEntries],
    SUM(me.[QuantityLiters]) AS [TodayQuantity],
    AVG(me.[QualityScore]) AS [AvgQuality]
FROM [dbo].[Farmers] f
LEFT JOIN [dbo].[MilkEntries] me ON f.[FarmerID] = me.[FarmerID] 
    AND me.[EntryDate] = CAST(GETDATE() AS DATE)
WHERE f.[Status] = 'ACTIVE'
GROUP BY f.[FarmerID], f.[FarmerCode], f.[FullName];
```

**3. Quality Control Report**
```sql
SELECT 
    COUNT(*) AS [TotalEntries],
    SUM(CASE WHEN me.[QualityStatus] = 'APPROVED' THEN 1 ELSE 0 END) AS [Approved],
    SUM(CASE WHEN me.[QualityStatus] = 'REJECTED' THEN 1 ELSE 0 END) AS [Rejected],
    CAST(100.0 * 
        SUM(CASE WHEN me.[QualityStatus] = 'APPROVED' THEN 1 ELSE 0 END) / 
        COUNT(*) AS DECIMAL(5,2)) AS [ApprovalRate],
    SUM(CASE WHEN me.[IsAdulterated] = 1 THEN 1 ELSE 0 END) AS [AdulteratedCount]
FROM [dbo].[MilkEntries] me
WHERE me.[EntryDate] BETWEEN @StartDate AND @EndDate;
```

**4. Farmer Top 10 (by quantity)**
```sql
SELECT TOP 10
    f.[FarmerCode],
    f.[FullName],
    f.[FarmType],
    SUM(me.[QuantityLiters]) AS [TotalQuantity],
    AVG(me.[QualityScore]) AS [AvgQuality],
    COUNT(DISTINCT me.[EntryDate]) AS [CollectionDays]
FROM [dbo].[Farmers] f
INNER JOIN [dbo].[MilkEntries] me ON f.[FarmerID] = me.[FarmerID]
WHERE me.[EntryDate] BETWEEN DATEADD(MONTH, -1, CAST(GETDATE() AS DATE)) 
                         AND CAST(GETDATE() AS DATE)
    AND me.[IsDeleted] = 0
GROUP BY f.[FarmerID], f.[FarmerCode], f.[FullName], f.[FarmType]
ORDER BY [TotalQuantity] DESC;
```

---

## 🔧 Performance Tuning

### Monitor Query Performance
```sql
-- Identify slow queries
SELECT TOP 20 
    qt.text,
    qs.creation_time,
    qs.total_elapsed_time / 1000 AS total_time_ms,
    qs.execution_count,
    qs.total_elapsed_time / qs.execution_count / 1000 AS avg_time_ms
FROM sys.dm_exec_query_stats qs
INNER JOIN sys.dm_exec_sql_text qt ON qs.sql_handle = qt.sql_handle
ORDER BY qs.total_elapsed_time DESC;
```

### Check Index Fragmentation
```sql
SELECT 
    OBJECT_NAME(ips.object_id) AS [Table],
    i.name AS [Index],
    ips.avg_fragmentation_in_percent,
    CASE 
        WHEN ips.avg_fragmentation_in_percent < 10 THEN 'REBUILD'
        WHEN ips.avg_fragmentation_in_percent < 30 THEN 'REORGANIZE'
        ELSE 'OK'
    END AS [Recommendation]
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id
ORDER BY ips.avg_fragmentation_in_percent DESC;
```

### Rebuild Fragmented Indexes
```sql
-- Rebuild all fragmented indexes
ALTER INDEX ALL ON [dbo].[MilkEntries] REBUILD;
ALTER INDEX ALL ON [dbo].[Farmers] REBUILD;

-- Update statistics
UPDATE STATISTICS [dbo].[MilkEntries];
UPDATE STATISTICS [dbo].[Farmers];
```

---

## 📈 Scaling Recommendations

### When to Partition
```
Trigger: MilkEntries table > 50M rows
Solution: Partitioning by EntryDate (monthly or yearly)

-- Example: Partition by year
CREATE PARTITION FUNCTION pf_MilkEntriesByYear (DATE)
    AS RANGE RIGHT FOR VALUES ('2024-12-31', '2025-12-31', '2026-12-31');
```

### When to Use Columnstore
```
Trigger: Large aggregations > 5 second response
Solution: Nonclustered Columnstore Index

CREATE NONCLUSTERED COLUMNSTORE INDEX ix_mce_columnstore
    ON [dbo].[MilkEntries] (
        [EntryDate], [MilkType], [QuantityLiters], [QualityScore], [TotalPrice]
    );
```

### When to Archive
```
Trigger: Audit log or historical data > 1GB
Strategy: Move to separate archive database

-- Archive old audit logs (older than 1 year)
INSERT INTO [ArchiveDb].[dbo].[AuditLog]
SELECT * FROM [dbo].[AuditLog]
WHERE [ChangedDate] < DATEADD(YEAR, -1, GETDATE());

DELETE FROM [dbo].[AuditLog]
WHERE [ChangedDate] < DATEADD(YEAR, -1, GETDATE());
```

---

## 🔐 Backup & Recovery

### Backup Strategy
```sql
-- Full backup (daily)
BACKUP DATABASE [MilkCollectionSystemDb]
TO DISK = 'C:\Backups\MilkCollectionSystemDb_Full_20251222.bak'
WITH INIT, COMPRESSION;

-- Transaction log backup (every 15 min)
BACKUP LOG [MilkCollectionSystemDb]
TO DISK = 'C:\Backups\MilkCollectionSystemDb_Log_20251222.trn'
WITH INIT, COMPRESSION;

-- Restore
RESTORE DATABASE [MilkCollectionSystemDb]
FROM DISK = 'C:\Backups\MilkCollectionSystemDb_Full_20251222.bak'
WITH RECOVERY;
```

### Recovery Scenarios
```sql
-- Point-in-time recovery
RESTORE DATABASE [MilkCollectionSystemDb]
FROM DISK = 'C:\Backups\MilkCollectionSystemDb_Full_20251222.bak'
WITH RECOVERY, STOPAT = '2025-12-22 15:30:00';
```

---

## 🐛 Troubleshooting

### Issue: "Cannot insert into Farmers with NULL values"
**Solution:**
```sql
-- Check NOT NULL constraints
SELECT COLUMN_NAME, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Farmers';

-- FarmerCode, FirstName, LastName, Status are required
```

### Issue: "Foreign key constraint violation"
**Cause:** FarmerID doesn't exist in Farmers table
**Solution:**
```sql
-- Verify farmer exists
SELECT * FROM [dbo].[Farmers] WHERE [FarmerID] = @FarmerID;

-- Or use stored procedure which validates
EXEC [dbo].[sp_InsertMilkEntry] @FarmerID = 1, ...;
```

### Issue: "Query timeout"
**Solutions:**
```sql
-- 1. Add missing index
CREATE INDEX idx_missing ON [dbo].[MilkEntries]([EntryDate]);

-- 2. Rebuild fragmented indexes
ALTER INDEX idx_name ON [dbo].[MilkEntries] REBUILD;

-- 3. Update statistics
UPDATE STATISTICS [dbo].[MilkEntries];

-- 4. Increase timeout (C#)
context.Database.SetCommandTimeout(300); // 5 minutes
```

---

## 📞 Support & Documentation

- **Schema File:** `Database_Schema_Production.sql`
- **Architecture Guide:** `DATABASE_ARCHITECTURE_GUIDE.md`
- **Sample Data:** Insert statements in each table section
- **Version:** 1.0 (Dec 22, 2025)

---

## ✅ Post-Implementation Checklist

- [ ] Database created successfully
- [ ] All tables visible in SSMS
- [ ] All indexes created
- [ ] All stored procedures created
- [ ] All views working
- [ ] Quality thresholds inserted
- [ ] Test farmer created
- [ ] Test milk entry created
- [ ] Stored procedure tested
- [ ] Views return data
- [ ] Backup tested
- [ ] C# connection working
- [ ] API endpoints tested
- [ ] Performance baseline established

---

**🎉 Ready to use! Your database is production-ready.**
