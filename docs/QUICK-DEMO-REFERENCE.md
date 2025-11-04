# 快速展示參考指南

> 面試展示快速參考，建議列印或保存在手機

## 🚀 一鍵啟動

```bash
scripts\啟動展示系統-Cloudflare.bat
```

等待所有服務啟動（約 30-60 秒）

---

## 🌐 公開網址

查看這三個視窗，找到公開網址：

### 1️⃣ React 管理後台（主要展示）
**視窗**：【NGO】Cloudflare Tunnel - 前端 React (5173)

**網址格式**：`https://xxxxx-xxxxx-xxxxx.trycloudflare.com`

**給面試官的網址** ⭐

### 2️⃣ MVC 用戶前台（補充展示）
**視窗**：【NGO】Cloudflare Tunnel - MVC 前端 (5066)

**網址格式**：`https://xxxxx-xxxxx-xxxxx.trycloudflare.com`

### 3️⃣ 後端 API（技術說明用）
**視窗**：【NGO】Cloudflare Tunnel - 後端 API (5264)

**網址格式**：`https://xxxxx-xxxxx-xxxxx.trycloudflare.com`

---

## 🔐 測試帳號

| 角色 | 帳號 | 密碼 |
|------|------|------|
| 管理員 | `admin@ngo.org` | `Admin123!` |
| 督導 | `supervisor@ngo.org` | `Super123!` |
| 員工 | `staff@ngo.org` | `Staff123!` |

**建議使用管理員帳號展示**

---

## 🎯 展示流程（5 分鐘）

### 1. 開場（30秒）
```
"這是一個完整的 NGO 管理系統，採用前後端分離架構。
後端使用 .NET 9 WebAPI，前端有 React 管理後台和 MVC 用戶前台。
資料庫使用 SQL Server，目前有 32 個個案、24 位志工。"
```

### 2. 登入系統（15秒）
- 開啟 React 管理後台網址
- 輸入：`admin@ngo.org` / `Admin123!`
- 點擊登入

### 3. Dashboard 展示（1分鐘）
**說明重點**：
- "這是即時統計儀表板"
- "顯示個案總數、志工數、活動數"
- "性別分佈圖表使用 Chart.js"
- "城市分佈涵蓋 18 個縣市"

### 4. 案件管理（1分鐘）
**操作流程**：
- 點擊左側選單「案件管理」
- 展示案件列表
- 點擊「查看」展示案件詳情

**說明重點**：
- "完整的 CRUD 功能"
- "案件狀態追蹤"
- "關聯志工資訊"

### 5. 活動管理（1分鐘）
**操作流程**：
- 點擊「活動管理」
- 展示活動列表
- 點擊「查看詳情」

**說明重點**：
- "活動建立與管理"
- "報名人數追蹤"
- "活動狀態管理"

### 6. 其他功能快覽（30秒）
快速點擊展示：
- 物資管理
- 帳號管理
- 行程管理

### 7. 技術亮點（1分鐘）
```
"技術特點：
1. 前後端完全分離，RESTful API 設計
2. 使用 Entity Framework Core 的 Code First
3. React + Material-UI 現代化界面
4. JWT Token 身份驗證
5. 角色權限管理系統
6. 響應式設計，支援各種螢幕"
```

### 8. MVC 前台展示（選用，1分鐘）
**開啟 MVC 網址**

**說明重點**：
```
"這是用戶前台，使用 ASP.NET Core MVC。
設計理念是：管理後台用 React（適合複雜互動），
用戶前台用 MVC（適合 SEO 和快速開發）。
兩個前端共用同一個後端 API。"
```

---

## 💬 常見技術問題

### Q1: 為什麼選擇前後端分離？
```
A:
1. 前端可以獨立開發和部署
2. API 可以被多個前端共用（React、MVC、未來的 App）
3. 技術棧選擇更靈活
4. 更容易擴展和維護
```

### Q2: 為什麼用雙前端架構？
```
A:
1. 管理後台需求：複雜的互動、豐富的圖表 → React 更適合
2. 用戶前台需求：SEO、整合第三方服務 → MVC 更適合
3. 各自發揮優勢，提供最佳用戶體驗
```

### Q3: 資料庫設計特點？
```
A:
1. 標準化設計，避免資料冗餘
2. 20 張資料表，涵蓋完整業務流程
3. 使用外鍵確保資料完整性
4. 2 個 Views 簡化複雜查詢
5. 1 個 Trigger 自動維護資料
```

### Q4: 如何保證 API 安全？
```
A:
1. JWT Token 身份驗證
2. HTTPS 加密傳輸
3. CORS 嚴格設定
4. 輸入驗證防止注入攻擊
5. 角色權限控制
6. EF Core 參數化查詢防止 SQL Injection
```

### Q5: 做了哪些效能優化？
```
A:
1. 後端：EF Core 查詢優化，避免 N+1 問題
2. 前端：React 的 lazy loading
3. 圖表資料分頁載入
4. Vite 的快速 HMR
5. 資料庫索引優化
```

### Q6: 如何處理錯誤？
```
A:
1. 後端：全局異常處理中介軟體
2. 前端：錯誤邊界（Error Boundary）
3. API 統一錯誤格式
4. 友善的錯誤訊息提示
5. 日誌記錄追蹤
```

---

## ⚠️ 注意事項

### 展示前檢查

- [ ] 所有 6 個視窗都在運行
- [ ] 記下三個公開網址
- [ ] 測試管理員帳號可以登入
- [ ] 確認網路連線穩定
- [ ] 準備好技術問題答案

### 展示中注意

- ✅ 保持自信，操作流暢
- ✅ 說話清晰，語速適中
- ✅ 強調技術亮點
- ✅ 控制時間在 5-7 分鐘
- ❌ 不要糾結於小瑕疵
- ❌ 不要展示未完成的功能

### 展示後

- 詢問面試官是否有問題
- 提供 GitHub 連結（如果有）
- 感謝面試官的時間

---

## 🛠 緊急處理

### 如果網址無法訪問

1. **檢查服務是否運行**
   ```bash
   # 查看進程
   tasklist | findstr "dotnet node cloudflared"
   ```

2. **檢查本地訪問**
   - 後端：http://localhost:5264
   - 前端：http://localhost:5175
   - MVC：http://localhost:5066

3. **重啟 Cloudflare Tunnel**
   ```bash
   # 關閉舊的
   taskkill /F /IM cloudflared.exe

   # 重新啟動
   cloudflared tunnel --url http://localhost:5264
   cloudflared tunnel --url http://localhost:5175
   cloudflared tunnel --url http://localhost:5066
   ```

### 如果無法登入

1. **確認後端正在運行**
2. **檢查 API 網址配置**
3. **使用備用帳號**：
   - 督導：`supervisor@ngo.org` / `Super123!`
   - 員工：`staff@ngo.org` / `Staff123!`

### 如果出現錯誤

**保持冷靜**：
```
"這裡遇到一個小問題，讓我看一下...
（快速檢查錯誤訊息）
這可能是網路延遲，我們先看下一個功能"
```

**備案**：
- 準備截圖或影片
- 切換到本地展示
- 說明技術架構

---

## 📱 手機備忘

### 關鍵網址
```
React: https://iron-publisher-encouraging-change...
MVC:   https://developed-stuck-indicators-bay...
API:   https://virtue-football-hunting-possible...
```

### 測試帳號
```
admin@ngo.org / Admin123!
```

### 展示時間
```
開場 30s → 登入 15s → Dashboard 1m →
案件 1m → 活動 1m → 快覽 30s → 技術 1m
= 5-6 分鐘
```

### 技術關鍵字
```
.NET 9 WebAPI
React 18
Entity Framework Core
JWT Token
Material-UI
SQL Server
前後端分離
RESTful API
```

---

## 🎉 加油打氣

你已經：
- ✅ 開發了完整的系統
- ✅ 準備了完善的文檔
- ✅ 配置了展示環境
- ✅ 規劃了展示流程

**你準備好了！相信自己！** 💪

記住：
- 這是**你的作品**，你最了解它
- 展示**核心功能**就好，不需要完美
- **自信**比技術更重要
- **溝通能力**也是考核重點

**祝你面試順利！加油！** 🚀

---

**列印提示**：建議列印本頁或保存到手機，面試前快速複習
