# 🌐 Cloudflare Tunnel 公開展示指南

> 無需註冊即可將本地系統公開展示給面試官或其他人

---

## 📖 為什麼使用 Cloudflare Tunnel？

相比 ngrok 的優勢：

| 特性 | Cloudflare Tunnel | ngrok |
|------|-------------------|-------|
| **免費額度** | 無限制 ✅ | 1個網址 |
| **警告頁面** | **無** ✅ | 有（影響展示） |
| **需要帳號** | **不需要** ✅ | 需要 |
| **網址穩定性** | 重啟前固定 | 每次重啟都變 |
| **速度** | 快（CDN） | 一般 |
| **設定難度** | 簡單 | 簡單 |

**結論**：Cloudflare Tunnel 更適合面試展示！

---

## ⚙️ 環境需求

### 安裝 cloudflared

```batch
winget install --id Cloudflare.cloudflared
```

**驗證安裝**：
```batch
cloudflared version
```

應該看到版本資訊（例如：2025.8.1）

---

## 🚀 快速啟動（推薦）

本系統提供**兩種啟動模式**，根據展示需求選擇：

---

### 方式 A：簡化版（4 個視窗）

**適用場景**：
- 快速展示 React 管理後台功能
- 面試重點在前端技術
- 時間有限（5-7 分鐘）

```batch
# 雙擊執行
scripts\啟動展示系統-簡化版.bat
```

這會自動啟動：
1. 後端 WebAPI (localhost:5264)
2. React 管理前端 (localhost:5173)
3. Cloudflare Tunnel - 後端 API
4. Cloudflare Tunnel - React 前端

啟動後會開啟 **4 個視窗**：
- 【NGO-簡化版】後端 WebAPI (5264)
- 【NGO-簡化版】React 管理前端 (5173)
- 【NGO-簡化版】Cloudflare Tunnel - 後端 API (5264)
- 【NGO-簡化版】Cloudflare Tunnel - React 前端 (5173)

---

### 方式 B：完整版（6 個視窗）- 推薦面試展示

**適用場景**：
- **完整系統展示**（推薦）
- 展示 **ECPay 金流整合**技術
- 展示前後台完整業務流程
- 時間充裕（10-15 分鐘）

```batch
# 雙擊執行（推薦）
scripts\啟動展示系統-完整版.bat
```

這會自動啟動：
1. 後端 WebAPI (localhost:5264)
2. React 管理後台 (localhost:5173)
3. MVC 用戶前台 (localhost:5066)
4. Cloudflare Tunnel - 後端 API
5. Cloudflare Tunnel - React 管理後台
6. Cloudflare Tunnel - MVC 用戶前台

啟動後會開啟 **6 個視窗**：
- 【NGO-完整版】後端 WebAPI (5264)
- 【NGO-完整版】React 管理後台 (5173)
- 【NGO-完整版】MVC 用戶前台 (5066)
- 【NGO-完整版】Cloudflare Tunnel - 後端 API (5264)
- 【NGO-完整版】Cloudflare Tunnel - React 管理後台 (5173)
- 【NGO-完整版】Cloudflare Tunnel - MVC 用戶前台 (5066)

**完整版優勢**：
- ✨ 展示雙前端架構（React + MVC）
- ✨ ECPay 綠界金流整合（CheckMacValue SHA256 驗證）
- ✨ Google OAuth 第三方登入
- ✨ 完整業務流程（民眾認購物資 → ECPay 付款 → 社工管理）
- ✨ 更全面的技術展示

---

## 🔧 手動啟動（完整步驟）

如果啟動腳本不可用，可以手動操作：

### 步驟 1: 啟動後端 API

開啟命令提示字元視窗 1：
```batch
cd C:\Users\lanli\source\repos\NGO-Management-System\backend
dotnet run
```

等待看到：`Now listening on: http://localhost:5264`

---

### 步驟 2: 啟動前端 React

開啟命令提示字元視窗 2：
```batch
cd C:\Users\lanli\source\repos\NGO-Management-System\frontend
npm run dev
```

等待看到：`Local: http://localhost:5173/`

---

### 步驟 3: 建立後端 Tunnel

開啟命令提示字元視窗 3：
```batch
cloudflared tunnel --url http://localhost:5264
```

**重要**：記下顯示的公開網址，例如：
```
https://virtue-football-hunting-possible.trycloudflare.com
```

---

### 步驟 4: 建立前端 Tunnel

開啟命令提示字元視窗 4：
```batch
cloudflared tunnel --url http://localhost:5173
```

**重要**：記下顯示的公開網址，例如：
```
https://iron-publisher-encouraging-change.trycloudflare.com
```

---

### 步驟 5: 更新前端配置

**重要步驟**：前端需要知道後端的公開網址

1. 打開前端配置檔：`frontend/.env.development`
2. 修改 API 網址為後端的 Cloudflare 網址：
   ```env
   VITE_API_BASE_URL=https://virtue-football-hunting-possible.trycloudflare.com/api
   ```
   （替換為步驟 3 中獲得的網址，記得加上 `/api`）
3. 儲存檔案
4. **重新啟動前端**（在視窗 2 按 Ctrl+C，然後再執行 `npm run dev`）

---

### 步驟 6: 記錄公開網址

將網址記錄到 `展示網址.md` 或其他地方：

```markdown
# NGO 系統展示網址

## 前端（給面試官的主要網址）
https://iron-publisher-encouraging-change.trycloudflare.com

## 後端 API
https://virtue-football-hunting-possible.trycloudflare.com

## 測試帳號
管理員： admin@ngo.org / Admin123!
```

---

### 步驟 7: 測試訪問

1. 開啟瀏覽器
2. 訪問**前端公開網址**
3. 使用測試帳號登入
4. 確認功能正常

---

## 🔄 每次重啟 Cloudflare 時的修改清單

⚠️ **重要**：Cloudflare Tunnel 每次重啟會產生**新的隨機網址**，必須更新以下配置！

### 📋 修改步驟（按順序執行）

#### 1️⃣ 記錄新的 Cloudflare 網址

**後端 Tunnel 網址**（步驟 3 獲得）：
```
例如：https://virtue-football-hunting-possible.trycloudflare.com
```

**前端 Tunnel 網址**（步驟 4 獲得）：
```
例如：https://iron-publisher-encouraging-change.trycloudflare.com
```

**MVC 前台 Tunnel 網址**（若有啟動）：
```
例如：https://extended-prep-garden-manufacture.trycloudflare.com
```

---

#### 2️⃣ 更新 React 前端配置

**檔案位置**：`NGO-Management-System/frontend/.env.development`

**修改內容**：
```env
# 修改為新的後端 Tunnel 網址（注意要加 /api）
VITE_API_BASE_URL=https://virtue-football-hunting-possible.trycloudflare.com/api
```

**執行動作**：
```bash
# 在前端終端機按 Ctrl+C 停止服務
# 然後重新啟動
npm run dev
```

---

#### 3️⃣ 更新 MVC 前台配置（若有啟動 MVC）

**檔案位置**：`NGO-Management-System/mvc-frontend/NGOPlatformWeb/appsettings.json`

**修改內容**：
```json
{
  "ECPay": {
    "UseNgrok": true,
    "NgrokUrl": "https://extended-prep-garden-manufacture.trycloudflare.com"
  }
}
```

**執行動作**：
```bash
# 在 MVC 終端機按 Ctrl+C 停止服務
# 然後重新啟動
dotnet run
```

---

#### 4️⃣ 測試連線

**檢查項目**：
- ✅ React 前端可以正常載入（訪問前端 Tunnel 網址）
- ✅ React 前端可以連接後端 API（登入測試）
- ✅ MVC 前台可以正常載入（若有啟動）
- ✅ ECPay 付款功能正常（若需要測試金流）

---

### 📝 快速修改檢查清單

複製此清單，每次重啟時逐項確認：

```
[ ] 記錄後端 Tunnel 網址
[ ] 記錄前端 Tunnel 網址
[ ] 記錄 MVC Tunnel 網址（若有）
[ ] 更新 frontend/.env.development 中的 VITE_API_BASE_URL
[ ] 重新啟動 React 前端（Ctrl+C → npm run dev）
[ ] 更新 mvc-frontend/.../appsettings.json 中的 NgrokUrl（若有 MVC）
[ ] 重新啟動 MVC 前台（Ctrl+C → dotnet run）（若有 MVC）
[ ] 測試 React 前端可訪問
[ ] 測試 React 登入功能
[ ] 測試 MVC 前台可訪問（若有）
[ ] 記錄新網址到展示文檔
```

---

### ⚡ 為什麼需要更新配置？

**原因**：
1. **Cloudflare Tunnel 的隨機網址**：每次重啟 `cloudflared` 會分配新的隨機網址
2. **前端硬編碼 API 位址**：React 前端在 `.env.development` 中指定 API 位址
3. **ECPay 回調網址**：MVC 前台需要告訴 ECPay 正確的回調網址

**不更新會怎樣**：
- ❌ React 前端無法連接後端（Network Error）
- ❌ 登入功能失效
- ❌ ECPay 付款回調失敗（MVC）

---

### 💡 減少修改的方法

#### 方法 1：使用付費 Cloudflare Tunnel（固定網址）

**優點**：網址永久不變，無需每次修改
**缺點**：需付費（約 $5/月）

#### 方法 2：僅在展示前啟動，展示期間不重啟

**優點**：一次設定即可
**缺點**：需要長時間保持電腦開機

#### 方法 3：使用環境變數或配置中心

**優點**：集中管理配置
**缺點**：需要額外開發

**推薦**：方法 2（展示前 30 分鐘啟動，展示期間不重啟）

---

## 📧 給面試官的郵件範本

```
主旨：NGO 管理系統展示 - [您的姓名]

您好，

這是我開發的 NGO 管理系統展示連結：

🔗 系統網址：https://iron-publisher-encouraging-change.trycloudflare.com

🔐 測試帳號：
管理員： admin@ngo.org
密碼： Admin123!

系統功能：
- 個案管理
- 活動管理
- 物資媒合
- 志工管理
- 統計報表

技術棧：
- 前端：React + TypeScript + Ant Design
- 後端：.NET 9 WebAPI + Entity Framework
- 資料庫：SQL Server

請隨時測試，如有任何問題歡迎聯絡我。

謝謝！
[您的姓名]
```

---

## ⚠️ 重要注意事項

### 1. 網址有效期

- **Quick Tunnel 模式**：網址在 Tunnel 進程運行期間有效
- **重啟會改變網址**：每次重啟 cloudflared 會得到新的隨機網址
- **建議**：展示前啟動一次，不要中途重啟

### 2. 展示時機

**最佳時機**：
- 面試前 30 分鐘啟動
- 確保面試期間保持運行
- 面試結束後可以關閉

**不建議**：
- 提前幾天就啟動（電腦需要一直開著）
- 臨時啟動（可能出現問題來不及處理）

### 3. 電腦需求

- 電腦必須保持開機
- 網路必須保持連接
- 4 個命令提示字元視窗必須保持開啟

### 4. 安全性

- 這是**臨時展示用**，不要用於正式環境
- 測試帳號密碼是公開的
- 不要放敏感真實資料

---

## 🛠 故障排除

### 問題 1: cloudflared 未安裝

```
錯誤：'cloudflared' is not recognized as an internal or external command

解決：
winget install --id Cloudflare.cloudflared
```

### 問題 2: Tunnel 連線失敗

```
錯誤：error="Unable to connect to the origin"

可能原因：
1. 後端或前端未啟動
2. Port 不正確

解決：
1. 確認應用程式正在運行
2. 確認 Port 是 5264 (後端) 和 5173 (前端)
```

### 問題 3: 前端無法連接後端

```
錯誤：Network Error 或 CORS error

解決：
1. 確認已更新前端配置的 API 網址
2. 確認後端 Tunnel 正在運行
3. 重新啟動前端服務
```

### 問題 4: 網址訪問出現 403 Forbidden

```
錯誤：403 Forbidden

解決：
檢查 vite.config.ts 中的 allowedHosts 設定：
server: {
  host: true,
  allowedHosts: ['.trycloudflare.com']
}
```

---

## 🎯 面試展示建議

### 展示流程（5-7 分鐘）

1. **開場介紹**（30 秒）
   - "這是一個 NGO 管理系統，解決三個核心痛點..."
   - 技術棧簡介

2. **登入展示**（30 秒）
   - 展示登入頁面
   - 使用管理員帳號登入

3. **Dashboard**（1 分鐘）
   - 統計數字
   - 圖表展示

4. **個案管理**（2 分鐘）
   - 新增個案
   - 查看個案詳情
   - 編輯個案

5. **其他功能**（1-2 分鐘）
   - 活動管理
   - 物資媒合
   - 志工管理（簡單帶過）

6. **技術亮點**（1-2 分鐘）
   - 前後端分離架構
   - JWT 認證
   - Entity Framework 複雜關聯
   - React 狀態管理

### 常見技術問題準備

**Q1: 為什麼選擇這個技術棧？**
```
A: .NET 有強大的 Entity Framework 處理複雜資料關聯，
   React 適合構建互動式管理介面，
   兩者都有豐富的生態系統和社群支援。
```

**Q2: 如何處理 API 安全性？**
```
A: 使用 JWT Token 認證，
   實作角色權限控制（RBAC），
   API 端點都需要驗證 Token。
```

**Q3: 遇到什麼技術挑戰？**
```
A: 主要是處理複雜的資料表關聯，
   例如個案與服務記錄的多對多關係，
   使用 Entity Framework 的 Include 和 ThenInclude 解決。
```

---

## 🔄 停止服務

展示完成後，按順序關閉：

```batch
# 在每個命令提示字元視窗按 Ctrl + C

# 或使用以下命令一次關閉所有
taskkill /F /IM cloudflared.exe
taskkill /F /IM node.exe
taskkill /F /IM dotnet.exe
```

---

## 📝 檢查清單

展示前確認：

```
[ ] cloudflared 已安裝
[ ] 所有服務已啟動（後端、前端、兩個 Tunnel）
[ ] 前端配置已更新為後端公開網址
[ ] 已記錄公開網址
[ ] 已測試可以訪問
[ ] 已測試可以登入
[ ] 已測試核心功能運作正常
[ ] 已將網址提供給面試官
[ ] 電腦和網路保持穩定
```

---

<div align="center">

**預估設定時間**：10-15 分鐘（首次）

**展示準備度**：✅ 完全可用

**最後更新**：2025-12-18

</div>

---

**相關文檔**：
- [00-快速開始.md](00-快速開始.md) - 本地開發啟動
- [03-測試帳號.md](03-測試帳號.md) - 所有測試帳號
- [04-常見問題.md](04-常見問題.md) - 故障排除
