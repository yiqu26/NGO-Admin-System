# 🚀 啟動腳本說明

本資料夾包含各種啟動和設置腳本，方便快速啟動系統。

---

## 📋 腳本列表

### 🎯 主要啟動腳本

#### 1. `啟動展示系統.bat`
**用途**：完整展示模式啟動（推薦用於展示）

**啟動內容**：
- ✅ WebAPI 後端 (localhost:5264)
- ✅ React 前端 (localhost:5173)
- ✅ ngrok 後端 tunnel (公開 API)
- ✅ ngrok 前端 tunnel (公開前端)

**使用場景**：
- 需要給面試官展示系統
- 需要公開網址讓他人訪問
- 展示完整功能

**使用方式**：
```batch
雙擊執行： 啟動展示系統.bat
```

**啟動後**：
1. 自動開啟 4 個命令提示字元視窗
2. 瀏覽器自動開啟 ngrok 管理介面 (http://localhost:4040)
3. 在 ngrok 介面查看公開網址

**注意事項**：
- ⚠️ 展示期間需保持電腦開機
- ⚠️ 所有視窗不能關閉
- ⚠️ ngrok 免費版每次重啟網址會改變

---

#### 2. `重新啟動展示系統.bat`
**用途**：重新啟動展示系統

**使用場景**：
- 系統出現錯誤需要重啟
- ngrok 斷線需要重新啟動

**使用方式**：
```batch
雙擊執行： 重新啟動展示系統.bat
```

---

#### 3. `啟動NGO展示系統.bat`
**用途**：NGO 展示模式（與 `啟動展示系統.bat` 類似）

**功能**：啟動完整展示環境

**使用方式**：
```batch
雙擊執行： 啟動NGO展示系統.bat
```

---

#### 4. `start-demo.bat`
**用途**：Demo 模式（僅啟動後端和 ngrok API）

**啟動內容**：
- ✅ WebAPI 後端 (localhost:5264)
- ✅ ngrok HTTP tunnel (公開 API)

**使用場景**：
- 只需要展示後端 API
- 前端已部署到 Vercel
- 需要公開 API 供 Vercel 前端呼叫

**使用方式**：
```batch
雙擊執行： start-demo.bat
```

**啟動後**：
- 取得 ngrok 公開網址
- 將網址設定到 Vercel 環境變數 `VITE_API_BASE_URL`

---

#### 5. `start-ngrok-tunnel.bat`
**用途**：僅啟動 ngrok TCP tunnel（資料庫連線用）

**啟動內容**：
- ✅ ngrok TCP tunnel (port 1433 - SQL Server)

**使用場景**：
- 後端部署到 Render，需要連接本地資料庫
- 允許遠端服務連接本地 SQL Server

**使用方式**：
```batch
雙擊執行： start-ngrok-tunnel.bat
```

**啟動後**：
1. 記下 ngrok 提供的 TCP 地址（例如：`0.tcp.ngrok.io:12345`）
2. 建立連線字串：
   ```
   Server=0.tcp.ngrok.io,12345;Database=NGOPlatformDb;User Id=sa;Password=你的密碼;TrustServerCertificate=True;
   ```
3. 將連線字串設定到 Render 環境變數

**前置需求**：
- SQL Server 必須啟用 SQL 驗證
- 必須設定 `sa` 帳號密碼

---

### ⚙️ 設置腳本

#### 6. `setup-github-repos.bat`
**用途**：初始化 Git 並推送到 GitHub

**功能**：
- 初始化 Git repository
- 添加遠端倉庫
- 提交並推送代碼

**使用場景**：
- 第一次設置 GitHub
- 需要重新配置 Git

**使用方式**：
```batch
雙擊執行： setup-github-repos.bat
```

**使用前**：
1. 在 GitHub 建立新的 repository
2. 取得 repository URL
3. 修改腳本中的 URL

---

## 🎯 使用建議

### 本地開發
**不建議使用腳本**，建議手動啟動以便查看詳細日誌：
```batch
# 終端機 1
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run

# 終端機 2
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev
```

### 展示系統（本地公開）
**推薦使用**：`啟動展示系統.bat`
- 一鍵啟動所有服務
- 自動取得公開網址
- 適合展示給面試官

### 展示系統（雲端部署）
**推薦使用**：`start-demo.bat`
- 僅啟動後端和 ngrok
- 前端使用 Vercel 部署
- 節省本地資源

### 資料庫遠端連線
**推薦使用**：`start-ngrok-tunnel.bat`
- 允許 Render 後端連接本地資料庫
- 不需要遷移資料庫

---

## 🔧 腳本修改指南

### 修改啟動延遲
找到腳本中的 `timeout /t X`，修改 X 為想要的秒數：
```batch
timeout /t 15 /nobreak >nul
```

### 修改專案路徑
找到 `cd /d` 命令，修改路徑：
```batch
cd /d "C:\你的路徑\NGO-Admin-System-WebAPI"
```

### 修改 ngrok 設定
編輯 `config\ngrok-config.yml` 或腳本中的 ngrok 命令：
```batch
ngrok http 5264 --log=stdout
```

---

## 🚨 常見問題

### Q1: 腳本執行失敗
**可能原因**：
1. 路徑不正確
2. 相關程式未安裝（dotnet, npm, ngrok）
3. Port 被占用

**解決方式**：
- 檢查錯誤訊息
- 確認程式已安裝
- 關閉占用 port 的程式

### Q2: ngrok 顯示錯誤
**可能原因**：
1. 未設定 authtoken
2. 網路連線問題
3. ngrok 服務問題

**解決方式**：
```batch
# 設定 authtoken
ngrok config add-authtoken YOUR_TOKEN

# 測試 ngrok
ngrok http 5264
```

### Q3: 視窗一閃而過
**原因**：腳本執行出錯自動關閉

**解決方式**：
- 右鍵腳本 → 編輯
- 在最後加上 `pause` 指令查看錯誤訊息

---

## 📚 相關文檔

- [快速啟動指南](../QUICK-START.md)
- [本地開發環境啟動指南](../docs/本地開發環境啟動指南.md)
- [展示系統使用說明](../docs/展示系統使用說明.md)
- [部署指南](../docs/部署指南-免費方案.md)

---

## 🎉 總結

### 腳本選擇流程圖

```
需要做什麼？
    │
    ├─ 本地開發
    │   └─ 手動啟動（不使用腳本）
    │
    ├─ 展示給面試官（本地）
    │   └─ 使用「啟動展示系統.bat」
    │
    ├─ 展示系統（前端已部署）
    │   └─ 使用「start-demo.bat」
    │
    ├─ 遠端連接本地資料庫
    │   └─ 使用「start-ngrok-tunnel.bat」
    │
    └─ 設置 GitHub
        └─ 使用「setup-github-repos.bat」
```

---

**最後更新**：2025-11-01
**腳本版本**：v1.0
