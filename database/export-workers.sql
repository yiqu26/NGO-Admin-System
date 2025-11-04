-- 匯出 Workers 資料表（測試帳號）
-- 執行此腳本後，複製結果並保存

SET NOCOUNT ON;

-- 產生 PostgreSQL 格式的 INSERT 語句
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql +
    'INSERT INTO "Workers" ("WorkerId", "Name", "Email", "Password", "Role", "IsActive") VALUES (' +
    CAST(WorkerId AS NVARCHAR) + ', ' +
    '''' + REPLACE(Name, '''', '''''') + ''', ' +
    '''' + REPLACE(Email, '''', '''''') + ''', ' +
    '''' + REPLACE(Password, '''', '''''') + ''', ' +
    '''' + REPLACE(Role, '''', '''''') + ''', ' +
    CASE WHEN IsActive = 1 THEN 'true' ELSE 'false' END +
    ');' + CHAR(13) + CHAR(10)
FROM Workers
ORDER BY WorkerId;

PRINT '-- Workers 資料';
PRINT @sql;

-- 顯示統計
PRINT '';
PRINT '-- 統計資訊:';
SELECT 'Workers' AS TableName, COUNT(*) AS RecordCount FROM Workers;
