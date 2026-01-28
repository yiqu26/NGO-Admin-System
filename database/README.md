# 💾 資料庫腳本說明

本資料夾包含資料庫相關的匯出和遷移腳本。

---

## 📋 文件列表

### SQL 匯出腳本

#### 1. `export-data.sql`
**用途**：匯出完整資料庫資料

**包含內容**：
- Workers（社工帳號）
- Cases（個案資料）
- Activities（活動資料）
- Materials（物資資料）
- Schedules（行程資料）
- 其他相關表格

**使用方式**：
```sql
-- 在 SSMS 中執行
-- 或使用 sqlcmd
sqlcmd -S YUNYUE\SQLEXPRESS -d NGOPlatformDb -i export-data.sql -o output.txt
```

**使用場景**：
- 備份資料庫資料
- 遷移到另一個資料庫
- 資料分析

---

#### 2. `export-workers.sql`
**用途**：僅匯出社工帳號資料

**包含內容**：
- Workers 表格的所有資料
- 包含測試帳號

**使用方式**：
```sql
-- 在 SSMS 執行
USE NGOPlatformDb;
GO

-- 執行腳本
:r export-workers.sql
```

**使用場景**：
- 備份帳號資料
- 匯出測試帳號列表
- 分析用戶資料

**匯出格式範例**：
```sql
SELECT
    WorkerId,
    Email,
    Name,
    Role,
    CreatedAt
FROM Workers
ORDER BY WorkerId;
```

---

### PowerShell 遷移腳本

#### 3. `export-to-postgres.ps1`
**用途**：將 SQL Server 資料匯出到 PostgreSQL

**功能**：
- 從 SQL Server 讀取資料
- 轉換為 PostgreSQL 相容格式
- 匯入到 PostgreSQL 資料庫

**使用方式**：
```powershell
# 執行腳本
.\export-to-postgres.ps1

# 或指定參數
.\export-to-postgres.ps1 -SourceServer "YUNYUE\SQLEXPRESS" -TargetHost "localhost"
```

**前置需求**：
- 已安裝 PostgreSQL
- 已安裝 SQL Server PowerShell 模組
- 已設定資料庫連線

**使用場景**：
- 從 SQL Server 遷移到 PostgreSQL
- 雲端部署（使用免費的 PostgreSQL 服務）
- 測試跨資料庫相容性

---

## 🔧 腳本使用指南

### 匯出完整資料庫

**步驟 1：使用 SSMS**
1. 開啟 SQL Server Management Studio
2. 連線到 `YUNYUE\SQLEXPRESS`
3. 選擇資料庫 `NGOPlatformDb`
4. 開啟新查詢視窗
5. 貼上 `export-data.sql` 內容
6. 執行（F5）

**步驟 2：儲存結果**
- 查詢結果會顯示所有資料
- 可以匯出為 CSV、Excel 等格式

---

### 匯出社工帳號

**方法一：使用 SSMS**
```sql
-- 開啟新查詢視窗
-- 執行 export-workers.sql
-- 結果 → 另存為...
```

**方法二：使用 sqlcmd**
```batch
sqlcmd -S YUNYUE\SQLEXPRESS -d NGOPlatformDb ^
  -Q "SELECT * FROM Workers" ^
  -o workers.txt -s "," -W
```

---

### 遷移到 PostgreSQL

**步驟 1：準備 PostgreSQL**
```sql
-- 連線到 PostgreSQL
psql -U postgres

-- 建立資料庫
CREATE DATABASE ngo_platform;

-- 建立對應的表格
-- (使用 Entity Framework 的 Migration 或手動建立)
```

**步驟 2：執行遷移腳本**
```powershell
# 編輯 export-to-postgres.ps1，設定連線參數
# 執行腳本
.\export-to-postgres.ps1
```

**步驟 3：驗證資料**
```sql
-- 在 PostgreSQL 檢查資料
SELECT COUNT(*) FROM workers;
SELECT COUNT(*) FROM cases;
```

---

## 📝 自訂匯出腳本

### 匯出特定日期範圍的資料

```sql
-- 匯出最近 30 天的個案
SELECT *
FROM Cases
WHERE CreatedDate >= DATEADD(day, -30, GETDATE())
ORDER BY CreatedDate DESC;
```

### 匯出統計資料

```sql
-- 個案統計
SELECT
    Status,
    COUNT(*) as Count
FROM Cases
GROUP BY Status;

-- 活動統計
SELECT
    ActivityType,
    COUNT(*) as Count
FROM Activities
GROUP BY ActivityType;
```

### 匯出關聯資料

```sql
-- 匯出個案及其負責社工
SELECT
    c.CaseId,
    c.Name as CaseName,
    w.Name as WorkerName,
    w.Email as WorkerEmail
FROM Cases c
LEFT JOIN Workers w ON c.WorkerId = w.WorkerId
ORDER BY c.CreatedDate DESC;
```

---

## 🚨 注意事項

### ⚠️ 資料安全

**敏感資料保護**：
- ❌ 不要將匯出的資料上傳到公開 GitHub
- ❌ 不要將密碼以明文儲存
- ✅ 匯出前先移除敏感資料
- ✅ 使用加密儲存匯出文件

**建議做法**：
```sql
-- 匯出時移除密碼欄位
SELECT
    WorkerId,
    Email,
    Name,
    Role,
    -- PasswordHash, -- 不要匯出密碼
    CreatedAt
FROM Workers;
```

---

### ⚠️ 備份建議

**定期備份**：
- 每週備份一次完整資料庫
- 每天備份增量資料
- 保留至少 3 個版本

**備份方式**：
```sql
-- 使用 SSMS
-- 右鍵資料庫 → 工作 → 備份

-- 或使用 T-SQL
BACKUP DATABASE NGOPlatformDb
TO DISK = 'C:\Backup\NGOPlatformDb_20251101.bak'
WITH FORMAT, COMPRESSION;
```

---

### ⚠️ 還原資料

**從備份還原**：
```sql
-- 還原資料庫
RESTORE DATABASE NGOPlatformDb
FROM DISK = 'C:\Backup\NGOPlatformDb_20251101.bak'
WITH REPLACE;
```

**從 SQL 腳本還原**：
```sql
-- 先清空表格
TRUNCATE TABLE Cases;
TRUNCATE TABLE Activities;
-- ... 其他表格

-- 執行匯入腳本
-- 使用 SSMS 或 sqlcmd
```

---

## 🔄 資料庫遷移流程

### SQL Server → PostgreSQL

**Step 1：匯出資料**
```batch
sqlcmd -S YUNYUE\SQLEXPRESS -d NGOPlatformDb ^
  -Q "SELECT * FROM Workers" -o workers.csv -s "," -W
```

**Step 2：轉換格式**
```powershell
# 使用 export-to-postgres.ps1
# 或手動修改 CSV
```

**Step 3：匯入 PostgreSQL**
```sql
COPY workers FROM 'workers.csv' DELIMITER ',' CSV HEADER;
```

---

### SQL Server → MySQL

**類似步驟**：
1. 匯出為 CSV
2. 修改資料格式（日期、布林值等）
3. 使用 `LOAD DATA INFILE` 匯入

---

## 📊 資料庫架構

### 主要表格

| 表格名稱 | 說明 | 主鍵 |
|---------|------|------|
| `Workers` | 社工帳號 | WorkerId |
| `Cases` | 個案資料 | CaseId |
| `Activities` | 活動資料 | ActivityId |
| `Materials` | 物資資料 | MaterialId |
| `Schedules` | 行程資料 | ScheduleId |
| `ActivityRegistrations` | 活動報名 | RegistrationId |

### 關聯關係

```
Workers (1) ─── (N) Cases
Workers (1) ─── (N) Schedules
Activities (1) ─── (N) ActivityRegistrations
Cases (1) ─── (N) ActivityRegistrations
```

---

## 🛠️ 實用查詢範例

### 查詢資料庫大小
```sql
SELECT
    name AS DatabaseName,
    size * 8 / 1024 AS SizeMB
FROM sys.master_files
WHERE database_id = DB_ID('NGOPlatformDb');
```

### 查詢表格資料筆數
```sql
SELECT
    t.name AS TableName,
    p.rows AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
ORDER BY p.rows DESC;
```

### 檢查資料完整性
```sql
-- 檢查是否有孤立的個案（沒有對應的社工）
SELECT *
FROM Cases c
LEFT JOIN Workers w ON c.WorkerId = w.WorkerId
WHERE w.WorkerId IS NULL;
```

---

## 📚 相關文檔

- [測試帳號列表](../docs/測試帳號列表.md)
- [本地開發環境啟動指南](../docs/本地開發環境啟動指南.md)
- [部署指南](../docs/部署指南-免費方案.md)

### 外部資源
- [SQL Server 備份文檔](https://docs.microsoft.com/sql/relational-databases/backup-restore/)
- [PostgreSQL COPY 文檔](https://www.postgresql.org/docs/current/sql-copy.html)
- [sqlcmd 工具文檔](https://docs.microsoft.com/sql/tools/sqlcmd-utility)

---

## 💡 使用提示

### 提示 1: 定期備份
建議使用 Windows 工作排程器自動執行備份：
```batch
# 建立批次檔 backup.bat
sqlcmd -S YUNYUE\SQLEXPRESS -Q "BACKUP DATABASE NGOPlatformDb TO DISK = 'C:\Backup\NGOPlatformDb_%date%.bak'"

# 加入工作排程器
schtasks /create /tn "NGO Database Backup" /tr "C:\path\to\backup.bat" /sc daily /st 02:00
```

### 提示 2: 匯出前檢查
在匯出前先檢查資料：
```sql
-- 檢查資料筆數
SELECT COUNT(*) FROM Workers;
SELECT COUNT(*) FROM Cases;

-- 檢查最新資料
SELECT TOP 10 * FROM Workers ORDER BY CreatedAt DESC;
```

### 提示 3: 壓縮備份
大型資料庫建議使用壓縮：
```sql
BACKUP DATABASE NGOPlatformDb
TO DISK = 'C:\Backup\NGOPlatformDb.bak'
WITH COMPRESSION;
```

---

**最後更新**：2025-11-01
**腳本版本**：v1.0
