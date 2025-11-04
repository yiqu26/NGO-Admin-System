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
# 編輯 appsettings.json，填入你的資料庫連線字串
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

### 選項 A：使用自動化腳本（推薦）

**展示模式（含公開網址）**
```batch
雙擊執行： scripts\啟動展示系統-Cloudflare.bat
```
✅ 自動啟動前端、後端和 Cloudflare Tunnel
✅ 取得公開網址供他人訪問

**本地開發模式**
需要手動開啟三個終端機（見下方）

---

### 選項 B：手動啟動（3 個終端機）

#### 🔴 終端機 1 - 後端 API（必須先啟動）
```batch
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run
```
**等待看到**：`Now listening on: http://localhost:5264`

---

#### 🟢 終端機 2 - 前端 React
```batch
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev
```
**等待看到**：`Local: http://localhost:5173/`

---

#### 🔵 終端機 3 - MVC 前台（選用）
```batch
cd C:\Users\lanli\source\repos\NGO-User-Case\NGOPlatformWeb
dotnet run
```

---

## 🔗 訪問系統

### 本地訪問
| 服務 | 網址 |
|------|------|
| React 後台 | http://localhost:5173 |
| WebAPI | http://localhost:5264 |
| MVC 前台 | http://localhost:5066 |

### 公開訪問（使用 ngrok）
執行 `scripts\啟動展示系統.bat` 後：
1. 開啟瀏覽器訪問 http://localhost:4040
2. 查看 ngrok 提供的公開網址
3. 前端網址（5173）- 給面試官看
4. 後端網址（5264）- 供前端呼叫 API

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
- [ ] （展示模式）已安裝並設定 ngrok

---

## 🚨 遇到問題？

### 問題 1: WebAPI 啟動失敗
```
解決：確認 SQL Server 正在運行
檢查：開啟 SSMS 確認可連線到 YUNYUE\SQLEXPRESS
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
   ↓
5. (展示用) ngrok tunnels
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

### 提示 1: 最快啟動
如果只是要測試系統，使用自動化腳本最快：
```batch
scripts\啟動展示系統.bat
```

### 提示 2: 停止服務
在終端機按 `Ctrl + C` 停止服務

### 提示 3: 重新啟動
如果遇到問題，關閉所有終端機，重新按順序啟動

### 提示 4: 查看日誌
- WebAPI：終端機會顯示 API 請求日誌
- React：瀏覽器 Console (F12) 顯示前端日誌

---

## 📚 需要更多資訊？

### 詳細指南
- [本地開發環境啟動指南](docs/本地開發環境啟動指南.md)
- [展示系統使用說明](docs/展示系統使用說明.md)
- [測試帳號列表](docs/測試帳號列表.md)

### 部署相關
- [部署指南-免費方案](docs/部署指南-免費方案.md)

### 工作總結
- [工作總結與下次啟動指南](docs/工作總結與下次啟動指南.md)

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

**ngrok 配置**
```
位置： config\ngrok-config.yml
內容： authtoken、tunnel 設定
```

---

## 🎉 啟動成功！

啟動後你可以：
- ✅ 使用 React 後台管理個案、活動、物資
- ✅ 測試不同角色的權限差異
- ✅ 查看 Dashboard 統計圖表
- ✅ 使用 ngrok 分享給面試官

---

## 📞 下一步

### 本地開發
繼續開發功能，測試各模組

### 展示準備
1. 執行展示腳本
2. 取得 ngrok 網址
3. 準備展示內容（5-10 分鐘）

### 正式部署
參考 [部署指南](docs/部署指南-免費方案.md) 部署到 Vercel + Render

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
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run

# 前端
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev

# 展示模式
scripts\啟動展示系統.bat

# 查看 ngrok
http://localhost:4040
```

---

**返回**：[主 README](README.md) | **詳細指南**：[docs/](docs/)
