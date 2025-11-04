# Cloudflare Tunnel 展示指南

> 使用免費的 Cloudflare Tunnel 將本地系統公開到網際網路，適合面試展示或遠端演示

## 📋 目錄

- [為什麼使用 Cloudflare Tunnel](#為什麼使用-cloudflare-tunnel)
- [快速啟動](#快速啟動)
- [手動啟動步驟](#手動啟動步驟)
- [故障排除](#故障排除)
- [注意事項](#注意事項)

---

## 🎯 為什麼使用 Cloudflare Tunnel

### Cloudflare Tunnel vs ngrok

| 特性 | Cloudflare Tunnel | ngrok (免費版) |
|------|-------------------|----------------|
| **免費額度** | ✅ 無限制 | ⚠️ 1 個網址 |
| **警告頁面** | ✅ 無 | ❌ 有警告頁面 |
| **網址穩定性** | ✅ 重啟前固定 | ❌ 每次重啟都變 |
| **速度** | ✅ 快速 (CDN) | ⚠️ 一般 |
| **需要註冊** | ✅ 不需要 | ❌ 需要 |
| **適合展示** | ✅ 非常適合 | ⚠️ 有警告頁面 |

**結論**：Cloudflare Tunnel 更適合面試展示，沒有警告頁面，體驗更專業。

---

## 🚀 快速啟動

### 方法一：使用啟動腳本（推薦）

```bash
# 雙擊執行或在命令列執行
scripts\啟動展示系統-Cloudflare.bat
```

這個腳本會自動：
1. 清理舊的進程
2. 啟動後端 API (Port 5264)
3. 啟動 React 前端 (Port 5173)
4. 啟動 MVC 前端 (Port 5066)
5. 為每個服務建立 Cloudflare Tunnel
6. 顯示公開網址

### 預期結果

腳本執行後，你會看到 **6 個命令視窗**：

1. **【NGO】後端 WebAPI (localhost:5264)** - 後端服務
2. **【NGO】前端 React (localhost:5173)** - React 管理後台
3. **【NGO】MVC 前端 (localhost:5066)** - MVC 用戶前台
4. **【NGO】Cloudflare Tunnel - 後端 API** - 顯示後端公開網址
5. **【NGO】Cloudflare Tunnel - 前端 React** - 顯示前端公開網址
6. **【NGO】Cloudflare Tunnel - MVC 前端** - 顯示 MVC 公開網址

### 查看公開網址

在 Cloudflare Tunnel 視窗中，找到類似這樣的訊息：

```
+--------------------------------------------------------------------------------------------+
|  Your quick Tunnel has been created! Visit it at (it may take some time to be reachable):  |
|  https://xxxxx-xxxxx-xxxx.trycloudflare.com                                                |
+--------------------------------------------------------------------------------------------+
```

**這就是你的公開網址！**

---

## 📝 手動啟動步驟

如果你想更了解運作原理，或需要客製化設定，可以手動啟動：

### 步驟 1：安裝 Cloudflared

```bash
# 使用 winget (Windows)
winget install --id Cloudflare.cloudflared

# 驗證安裝
cloudflared version
```

### 步驟 2：啟動後端 API

```bash
# 終端機 1
cd NGO-Admin-System-WebAPI
dotnet run
# 應該顯示：Now listening on: http://localhost:5264
```

### 步驟 3：啟動 React 前端

```bash
# 終端機 2
cd NGO-Admin-System
npm run dev
# 應該顯示：Local: http://localhost:5173/
```

### 步驟 4：啟動 MVC 前端（選用）

```bash
# 終端機 3
cd NGO-User-Case/NGOPlatformWeb
dotnet run
# 應該顯示：Now listening on: http://localhost:5066
```

### 步驟 5：為後端建立 Cloudflare Tunnel

```bash
# 終端機 4
cloudflared tunnel --url http://localhost:5264
```

等待幾秒鐘，你會看到公開網址：
```
https://virtue-football-hunting-possible.trycloudflare.com
```

**重要**：記下這個後端網址！

### 步驟 6：更新前端配置

在啟動前端 Tunnel 之前，需要更新前端的 API 配置：

1. 複製 `.env.development` 為 `.env.cloudflare`
2. 修改 `VITE_API_BASE_URL` 為步驟 5 的後端網址：

```env
# .env.cloudflare
VITE_API_BASE_URL=https://你的後端網址.trycloudflare.com/api
```

3. 重新啟動 React 前端（使用 cloudflare 模式）：

```bash
# 終端機 2（停止原本的前端）
# Ctrl+C 停止

# 使用 cloudflare 模式重新啟動
npm run dev -- --mode cloudflare
```

### 步驟 7：為前端建立 Cloudflare Tunnel

```bash
# 終端機 5
cloudflared tunnel --url http://localhost:5173
```

你會看到前端公開網址：
```
https://iron-publisher-encouraging-change.trycloudflare.com
```

**這是給面試官的主要網址！**

### 步驟 8：為 MVC 前端建立 Tunnel（選用）

```bash
# 終端機 6
cloudflared tunnel --url http://localhost:5066
```

MVC 前端公開網址：
```
https://developed-stuck-indicators-bay.trycloudflare.com
```

---

## 🎯 給面試官的展示流程

### 展示前準備

1. ✅ 確認所有服務都在運行
2. ✅ 記下三個公開網址
3. ✅ 準備測試帳號：`admin@ngo.org` / `Admin123!`
4. ✅ 測試登入一次，確保可以訪問

### 展示順序

#### 1. React 管理後台（主要展示）

**網址**：Cloudflare Tunnel - 前端 React 視窗中的網址

**展示流程**（3-5 分鐘）：
1. 開場介紹（30秒）
   - 這是一個完整的 NGO 管理系統
   - 採用前後端分離架構
   - .NET 9 + React + SQL Server

2. 登入系統（15秒）
   - 使用管理員帳號登入

3. Dashboard 展示（1分鐘）
   - 即時統計數據
   - 性別分佈圖表
   - 城市分佈統計
   - 今年新增案件趨勢

4. 案件管理（1分鐘）
   - 案件列表
   - 篩選功能
   - 快速查看案件詳情
   - 案件狀態管理

5. 活動管理（1分鐘）
   - 活動建立
   - 活動列表
   - 報名管理

6. 其他功能快速瀏覽（1分鐘）
   - 物資管理
   - 帳號管理
   - 行程管理

#### 2. MVC 用戶前台（補充展示）

**網址**：Cloudflare Tunnel - MVC 前端視窗中的網址

**展示重點**（1-2 分鐘）：
- 說明雙前端架構的設計理念
- 展示用戶視角的界面
- 活動報名流程
- 物資捐贈功能

#### 3. 技術問答準備

**可能的問題**：

**Q: 為什麼選擇前後端分離？**
A:
- 管理後台需要複雜的互動和圖表，React 更適合
- 用戶前台需要 SEO，MVC 更適合
- API 可以被多個前端共用
- 方便未來擴展（如行動 App）

**Q: 如何處理跨域問題？**
A:
- 後端設定 CORS policy
- 允許特定來源訪問
- 生產環境可以用 API Gateway

**Q: 資料庫設計有什麼特點？**
A:
- 標準化設計，避免資料冗餘
- 使用外鍵確保資料完整性
- Views 簡化複雜查詢
- Trigger 自動維護資料一致性

**Q: 有做效能優化嗎？**
A:
- Entity Framework 查詢優化
- 前端使用 lazy loading
- 圖表資料分頁載入
- Vite 的快速 HMR

---

## ⚠️ 注意事項

### 網址有效期

- Cloudflare Tunnel 的網址在**服務重啟前都是固定的**
- 如果重啟服務，網址會改變
- 建議面試前一次啟動完成，不要中途重啟

### 效能考量

- Cloudflare Tunnel 使用你的本地網路上傳頻寬
- 建議使用穩定的網路連線
- 如果網路不穩，可能會有延遲

### 安全提醒

- 這些是臨時的展示用網址
- 展示完畢後建議關閉服務
- 不要在這些網址上放敏感資料
- 測試帳號密碼僅供展示使用

### Port 衝突處理

如果遇到 Port 被占用：

```bash
# 查看 Port 使用情況
netstat -ano | findstr :5264
netstat -ano | findstr :5173
netstat -ano | findstr :5066

# 關閉占用 Port 的進程
taskkill /PID <PID> /F
```

### 前端無法連接後端

1. 確認後端 API 正在運行
2. 確認 `.env.cloudflare` 中的 API 網址正確
3. 確認使用 `--mode cloudflare` 啟動前端
4. 檢查瀏覽器 Console 的錯誤訊息

### Cloudflare Tunnel 連線失敗

1. 確認 cloudflared 已正確安裝
2. 檢查網路連線
3. 嘗試重新啟動 Tunnel
4. 確認本地服務正在運行

---

## 🛠 故障排除

### 問題 1：Cloudflared 未安裝

**錯誤訊息**：
```
'cloudflared' 不是內部或外部命令
```

**解決方法**：
```bash
winget install --id Cloudflare.cloudflared
```

### 問題 2：Port 已被占用

**錯誤訊息**：
```
Address already in use
```

**解決方法**：
```bash
# 關閉所有相關進程
taskkill /F /IM node.exe
taskkill /F /IM dotnet.exe
taskkill /F /IM cloudflared.exe

# 等待 3 秒
timeout /t 3

# 重新啟動
```

### 問題 3：前端無法連接後端 API

**症狀**：前端頁面載入但沒有資料

**檢查步驟**：
1. 確認後端 Cloudflare Tunnel 正在運行
2. 檢查 `.env.cloudflare` 中的 API 網址
3. 確認使用正確的模式啟動：`npm run dev -- --mode cloudflare`
4. 打開瀏覽器 Console 查看錯誤

### 問題 4：Vite 顯示 403 Forbidden

**原因**：Vite 沒有允許 Cloudflare 域名

**解決方法**：
在 `vite.config.ts` 中添加：
```typescript
server: {
  host: true,
  allowedHosts: ['.trycloudflare.com']
}
```

---

## 📚 進階設定

### 使用命名 Tunnel（長期使用）

如果需要固定的網址（例如長期展示），可以使用 Cloudflare 的命名 Tunnel：

1. 註冊 Cloudflare 帳號（免費）
2. 創建命名 Tunnel
3. 獲得固定的網址

詳細步驟：https://developers.cloudflare.com/cloudflare-one/connections/connect-apps

### 自訂網域

可以將 Cloudflare Tunnel 連接到你的自訂網域。

### 配置文件

可以使用配置文件來簡化 Tunnel 設定。

---

## 💡 最佳實踐

### 面試展示建議

1. **提前測試**：面試前至少測試一次完整流程
2. **準備備案**：如果網路出問題，準備截圖或錄影
3. **熟悉流程**：確保能在 3-5 分鐘內完成展示
4. **準備問題**：預想面試官可能問的技術問題
5. **展示重點**：專注於核心功能，不要太細節

### 錄製展示影片

建議錄製一個展示影片作為備份：

```bash
# 使用 OBS Studio 或其他錄影軟體
# 錄製重點功能操作
# 時長控制在 3-5 分鐘
```

---

## 📞 需要幫助？

如果遇到問題：

1. 查看本指南的「故障排除」章節
2. 查看 Cloudflare 官方文檔
3. 檢查系統日誌

---

## 🎉 總結

使用 Cloudflare Tunnel，你可以：

- ✅ 免費公開展示你的專案
- ✅ 沒有煩人的警告頁面
- ✅ 獲得穩定的公開網址
- ✅ 提供專業的展示體驗

**祝面試順利！** 🚀

---

**最後更新**：2025/11/04
