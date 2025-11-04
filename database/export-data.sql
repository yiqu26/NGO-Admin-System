-- NGO 資料庫匯出腳本
-- 用於從 SQL Server 匯出資料，稍後匯入到 PostgreSQL

USE NGOPlatformDb;
GO

-- 1. 匯出 Workers（員工/測試帳號）
SELECT 'Workers' AS TableName;
SELECT * FROM Workers;
GO

-- 2. 匯出 Cases（個案）
SELECT 'Cases' AS TableName;
SELECT * FROM Cases;
GO

-- 3. 匯出 Activities（活動）
SELECT 'Activities' AS TableName;
SELECT * FROM Activities;
GO

-- 4. 匯出 Supplies（物資）
SELECT 'Supplies' AS TableName;
SELECT * FROM Supplies;
GO

-- 5. 匯出 SupplyCategories（物資分類）
SELECT 'SupplyCategories' AS TableName;
SELECT * FROM SupplyCategories;
GO

-- 6. 匯出 Users（一般使用者）
SELECT 'Users' AS TableName;
SELECT * FROM Users;
GO

-- 7. 匯出 Schedules（行程）
SELECT 'Schedules' AS TableName;
SELECT * FROM Schedules;
GO

-- 統計資訊
SELECT
    'Workers' AS TableName, COUNT(*) AS RecordCount FROM Workers
UNION ALL
SELECT 'Cases', COUNT(*) FROM Cases
UNION ALL
SELECT 'Activities', COUNT(*) FROM Activities
UNION ALL
SELECT 'Supplies', COUNT(*) FROM Supplies
UNION ALL
SELECT 'Users', COUNT(*) FROM Users;
GO
