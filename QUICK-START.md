# ⚡ NGO 系統快速啟動指南

> 3 分鐘內啟動完整系統

---

## 🎯 最快啟動方式

### ⚙️ 首次設定（僅需一次）

**設定環境變數**
```batch
# 1. React 前端環境變數
cd NGO-Admin-System
copy .env.example .env.development

# 2. .NET 後端配置
cd ..\NGO-Admin-System-WebAPI
copy appsettings.example.json appsettings.json
# 編輯 appsettings.json，填入資料庫連線字串
```

**安裝依賴**
```batch
# React 前端
cd NGO-Admin-System
npm install

# .NET 後端（自動還原）
cd ..\NGO-Admin-System-WebAPI
dotnet restore
```

---

### 啟動步驟（3 個終端機）

#### 🔴 終端機 1 - 後端 API（必須先啟動）
```batch
cd NGO-Admin-System-WebAPI
dotnet run
```
**等待看到**：`Now listening on: http://localhost:5264`

---

#### 🟢 終端機 2 - 前端 React
```batch
cd NGO-Admin-System
npm run dev
```
**等待看到**：`Local: http://localhost:5173/`

---

#### 🔵 終端機 3 - MVC 前台（選用）
```batch
cd NGO-User-Case/NGOPlatformWeb
dotnet run
```

---

## 🔗 訪問系統

| 服務 | 網址 |
|------|------|
| React 後台 | http://localhost:5173 |
| WebAPI | http://localhost:5264 |
| MVC 前台 | http://localhost:5066 |

---

## 🔐 測試帳號（快速複製）

### 管理員（完整權限）
```
Email: admin@ngo.org
密碼: Admin123!
```

### 督導（管理權限）
```
Email: supervisor@ngo.org
密碼: Super123!
```

### 員工（基本權限）
```
Email: staff@ngo.org
密碼: Staff123!
```

---

## ✅ 啟動前檢查清單

- [ ] SQL Server 服務已啟動
- [ ] 確認資料庫 `NGOPlatformDb` 存在
- [ ] 已安裝 .NET 8.0 SDK
- [ ] 已安裝 Node.js 和 npm

---

## 🚨 遇到問題？

### 問題 1: WebAPI 啟動失敗
```
解決：確認 SQL Server 正在運行
檢查：開啟 SSMS 確認可連線到資料庫伺服器
```

### 問題 2: React 無法連接 API
```
解決：確認 WebAPI 已啟動（必須先啟動 WebAPI）
檢查：瀏覽器訪問 http://localhost:5264 是否有回應
```

### 問題 3: 登入失敗
```
解決：確認使用正確的測試帳號
檢查：Email 是 admin@ngo.org，密碼是 Admin123!（區分大小寫）
```

### 問題 4: Port 被占用
```
錯誤訊息：Address already in use
解決：關閉占用 port 的程式，或修改 port 設定
```

---

## 📊 系統啟動流程圖

```
啟動順序（必須按順序）：

1. SQL Server
   ↓
2. WebAPI 後端 (localhost:5264)
   ↓
3. React 前端 (localhost:5173)
   ↓
4. (選用) MVC 前台 (localhost:5066)
```

---

## 🎯 驗證系統正常運作

### Step 1: 測試後端
```batch
瀏覽器開啟： http://localhost:5264
應該看到： "NGO API 運作正常" 或類似訊息
```

### Step 2: 測試前端
```batch
瀏覽器開啟： http://localhost:5173
應該看到： 登入頁面
```

### Step 3: 測試登入
```
1. 輸入 Email: admin@ngo.org
2. 點擊「下一步」
3. 輸入密碼: Admin123!
4. 點擊「登入」
5. 應該進入 Dashboard 頁面
```

---

## 💡 快速提示

### 提示 1: 停止服務
在終端機按 `Ctrl + C` 停止服務

### 提示 2: 重新啟動
如果遇到問題，關閉所有終端機，重新按順序啟動

### 提示 3: 查看日誌
- WebAPI：終端機會顯示 API 請求日誌
- React：瀏覽器 Console (F12) 顯示前端日誌

---

## 🔧 環境配置位置

如需修改配置：

**後端 API 配置**
```
位置： NGO-Admin-System-WebAPI\appsettings.json
內容： 資料庫連線字串、JWT 設定、Azure 服務
```

**前端配置**
```
位置： NGO-Admin-System\.env.development
內容： API 位址、功能開關、檔案上傳限制
```

---

## 🎉 啟動成功！

啟動後可進行：
- ✅ 使用 React 後台管理個案、活動、物資
- ✅ 測試不同角色的權限差異
- ✅ 查看 Dashboard 統計圖表

---

<div align="center">

**預估啟動時間**：2-3 分鐘

**系統狀態**：✅ 已測試可正常使用

**最後更新**：2025-11-01

</div>

---

## 🌟 快速命令參考卡

```batch
# 後端
cd NGO-Admin-System-WebAPI
dotnet run

# 前端
cd NGO-Admin-System
npm run dev
```

---

**返回**：[主 README](README.md) | **詳細指南**：[docs/](docs/)
