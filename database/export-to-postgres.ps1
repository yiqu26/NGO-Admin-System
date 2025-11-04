# PowerShell 腳本：從 SQL Server 匯出資料並轉換為 PostgreSQL 格式
# ================================================================

param(
    [string]$ServerName = "YUNYUE\SQLEXPRESS",
    [string]$DatabaseName = "NGOPlatformDb",
    [string]$OutputFile = "C:\Users\lanli\source\repos\postgres-import.sql"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NGO 資料庫匯出工具" -ForegroundColor Cyan
Write-Host "從 SQL Server 匯出到 PostgreSQL 格式" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 SQL Server 連線
Write-Host "[1/5] 檢查 SQL Server 連線..." -ForegroundColor Yellow

$connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "✓ 成功連接到 SQL Server" -ForegroundColor Green
} catch {
    Write-Host "✗ 無法連接到 SQL Server: $_" -ForegroundColor Red
    exit 1
}

# 定義要匯出的資料表（按照外鍵順序）
$tables = @(
    "Workers",           # 員工（沒有外鍵，先匯出）
    "Cases",             # 個案（外鍵：WorkerId）
    "SupplyCategories",  # 物資分類（沒有外鍵）
    "Supplies",          # 物資（外鍵：SupplyCategoryId）
    "Activities",        # 活動（外鍵：WorkerId）
    "Users",             # 使用者（沒有外鍵）
    "Schedules",         # 行程（外鍵：WorkerId, CaseId）
    "News",              # 新聞
    "CaseLogins",        # 個案登入
    "RegularSuppliesNeeds",  # 定期物資需求
    "EmergencySupplyNeeds",  # 緊急物資需求
    "CaseActivityRegistrations",  # 個案活動報名
    "UserActivityRegistrations",  # 使用者活動報名
    "RegularDistributionBatch"    # 定期配送批次
)

Write-Host ""
Write-Host "[2/5] 開始匯出資料..." -ForegroundColor Yellow

$sqlOutput = @()
$sqlOutput += "-- PostgreSQL 匯入腳本"
$sqlOutput += "-- 產生時間: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$sqlOutput += "-- 來源: $ServerName\$DatabaseName"
$sqlOutput += ""
$sqlOutput += "-- 停用外鍵檢查（PostgreSQL 不支援，改用 transaction）"
$sqlOutput += "BEGIN;"
$sqlOutput += ""

foreach ($table in $tables) {
    Write-Host "  → 匯出資料表: $table" -ForegroundColor Cyan

    # 查詢資料
    $query = "SELECT * FROM [$table]"
    $command = $connection.CreateCommand()
    $command.CommandText = $query

    try {
        $reader = $command.ExecuteReader()
        $rowCount = 0

        # 取得欄位資訊
        $columns = @()
        for ($i = 0; $i -lt $reader.FieldCount; $i++) {
            $columns += $reader.GetName($i)
        }

        # 產生 INSERT 語句
        while ($reader.Read()) {
            $values = @()
            for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                $value = $reader.GetValue($i)

                if ($value -is [DBNull]) {
                    $values += "NULL"
                }
                elseif ($value -is [string]) {
                    $escapedValue = $value -replace "'", "''"
                    $values += "'$escapedValue'"
                }
                elseif ($value -is [DateTime]) {
                    $values += "'$($value.ToString('yyyy-MM-dd HH:mm:ss'))'"
                }
                elseif ($value -is [bool]) {
                    $values += if ($value) { "true" } else { "false" }
                }
                else {
                    $values += $value
                }
            }

            $columnList = ($columns | ForEach-Object { "`"$_`"" }) -join ", "
            $valueList = $values -join ", "
            $sqlOutput += "INSERT INTO `"$table`" ($columnList) VALUES ($valueList);"
            $rowCount++
        }

        $reader.Close()
        Write-Host "    ✓ 已匯出 $rowCount 筆資料" -ForegroundColor Green
        $sqlOutput += ""

    } catch {
        Write-Host "    ✗ 匯出失敗: $_" -ForegroundColor Red
    }
}

$sqlOutput += "COMMIT;"
$sqlOutput += ""
$sqlOutput += "-- 重置序列（PostgreSQL 自動遞增）"
foreach ($table in $tables) {
    $pkColumn = switch ($table) {
        "Workers" { "WorkerId" }
        "Cases" { "CaseId" }
        "Activities" { "ActivityId" }
        "Supplies" { "SupplyId" }
        "SupplyCategories" { "SupplyCategoryId" }
        "Users" { "UserId" }
        "Schedules" { "ScheduleId" }
        "News" { "NewsId" }
        default { $null }
    }

    if ($pkColumn) {
        $sqlOutput += "SELECT setval(pg_get_serial_sequence('`"$table`"', '$pkColumn'), COALESCE((SELECT MAX(`"$pkColumn`") FROM `"$table`"), 1), true);"
    }
}

Write-Host ""
Write-Host "[3/5] 關閉 SQL Server 連線..." -ForegroundColor Yellow
$connection.Close()
Write-Host "✓ 連線已關閉" -ForegroundColor Green

Write-Host ""
Write-Host "[4/5] 寫入檔案..." -ForegroundColor Yellow
$sqlOutput | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "✓ 已儲存到: $OutputFile" -ForegroundColor Green

Write-Host ""
Write-Host "[5/5] 統計資訊" -ForegroundColor Yellow
Write-Host "  檔案大小: $([math]::Round((Get-Item $OutputFile).Length / 1KB, 2)) KB" -ForegroundColor Cyan
Write-Host "  總行數: $($sqlOutput.Count)" -ForegroundColor Cyan

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ 匯出完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "下一步：" -ForegroundColor Yellow
Write-Host "1. 在 Render 建立 PostgreSQL 資料庫"
Write-Host "2. 使用 pgAdmin 或 psql 匯入此檔案"
Write-Host "3. 測試資料是否正確"
Write-Host ""
