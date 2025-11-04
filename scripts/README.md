# 🚀 NGO 展示系統 - 啟動腳本說明

本資料夾包含啟動腳本，方便快速啟動展示系統。

---

## 📋 腳本列表

### 🌟 主要啟動腳本

#### `啟動展示系統-Cloudflare.bat` ⭐ 推薦

**用途**：完整展示模式啟動（使用 Cloudflare Tunnel）

**啟動內容**：
- ✅ WebAPI 後端 (localhost:5264)
- ✅ React 前端 (localhost:5173)
- ✅ Cloudflare Tunnel 前端（公開網址）
- ✅ Cloudflare Tunnel 後端（公開網址）

**優點**：
- 🚀 比 ngrok 更穩定
- 🎯 沒有警告頁
- 💰 完全免費
- 🔗 網址更持久

**使用方式**：
```batch
雙擊執行： 啟動展示系統-Cloudflare.bat
```

**啟動後**：
1. 自動開啟 4 個命令提示字元視窗
2. 在 Cloudflare Tunnel 視窗中查看公開網址
3. 複製前端網址給面試官

**公開網址格式**：
```
https://random-words-1234.trycloudflare.com
```

**注意事項**：
- ⚠️ 展示期間需保持電腦開機
- ⚠️ 所有視窗不能關閉
- ✅ 網址在系統重啟前都是固定的

---

#### `setup-github-repos.bat`

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

## 🗂️ 資料夾說明

### `old-ngrok-backup/`

這個資料夾包含舊的 ngrok 相關腳本備份：
- 啟動展示系統.bat（多個版本）
- start-demo.bat
- start-ngrok-tunnel.bat
- ngrok 設定檔和說明

**保留原因**：
- 作為備份參考
- 萬一需要可以找回來
- 可以比較新舊腳本差異

**是否使用**：
- 不建議再使用 ngrok 腳本
- Cloudflare Tunnel 更穩定且免費
- 舊腳本僅供參考

---

## 🎯 使用建議

### 展示系統給面試官（推薦）

**使用**：`啟動展示系統-Cloudflare.bat`

**流程**：
1. 雙擊啟動腳本
2. 等待所有服務啟動（約 30 秒）
3. 在 Cloudflare Tunnel 視窗找到前端公開網址
4. 複製網址給面試官
5. 使用測試帳號登入展示

**測試帳號**：
- 管理員：admin@ngo.org / Admin123!
- 督導：supervisor@ngo.org / Super123!
- 員工：staff@ngo.org / Staff123!

---

### 本地開發

**不建議使用腳本**，建議手動啟動以便查看詳細日誌：

```batch
# 終端機 1 - 後端
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run

# 終端機 2 - 前端
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev
```

本地開發時直接使用：
- 前端：http://localhost:5173
- 後端：http://localhost:5264

---

## 🆚 Cloudflare Tunnel vs ngrok

| 特性 | Cloudflare Tunnel | ngrok (舊方案) |
|------|-------------------|----------------|
| 免費額度 | 無限制 | 1個網址 |
| 網址穩定性 | 重啟前固定 | 每次重啟都變 |
| 警告頁面 | 無 | 有（需要點擊繼續） |
| 速度 | 快（CDN） | 一般 |
| 設定複雜度 | 簡單 | 簡單 |
| **推薦度** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |

**結論**：強烈建議使用 Cloudflare Tunnel

---

## 🔧 故障排除

### Q1: cloudflared 找不到命令

**原因**：cloudflared 未安裝或未加入 PATH

**解決方式**：
```powershell
# 使用 winget 安裝
winget install --id Cloudflare.cloudflared

# 安裝後重新開啟終端機
```

---

### Q2: 腳本執行失敗

**可能原因**：
1. 路徑不正確
2. 相關程式未安裝（dotnet, npm, cloudflared）
3. Port 被占用

**解決方式**：
- 檢查錯誤訊息
- 確認程式已安裝：
  ```bash
  dotnet --version
  npm --version
  cloudflared version
  ```
- 關閉占用 port 的程式

---

### Q3: Cloudflare Tunnel 無法啟動

**可能原因**：
1. 網路連線問題
2. cloudflared 版本過舊

**解決方式**：
```powershell
# 更新 cloudflared
winget upgrade Cloudflare.cloudflared

# 測試 Cloudflare Tunnel
cloudflared tunnel --url http://localhost:5173
```

---

### Q4: 前端無法連接後端

**原因**：前端環境變數設定錯誤

**解決方式**：
檢查 NGO-Admin-System 的 .env 檔案：
```env
VITE_API_BASE_URL=http://localhost:5264
```

本地展示時應使用 localhost，不需要改成 Cloudflare 網址。

---

## 📚 相關文檔

- [快速啟動指南](../QUICK-START.md)
- [Cloudflare Tunnel 安裝指南](../Cloudflare-Tunnel-安裝指南.md)
- [清理完成指南](../清理完成-下一步指南.md)

---

## 🎉 總結

### 腳本選擇流程圖

```
需要做什麼？
    │
    ├─ 本地開發
    │   └─ 手動啟動（不使用腳本）
    │
    ├─ 展示給面試官
    │   └─ 使用「啟動展示系統-Cloudflare.bat」⭐
    │
    └─ 設置 GitHub
        └─ 使用「setup-github-repos.bat」
```

---

## 🎓 最佳實踐

### 面試前準備

1. **測試運行**：
   - 面試前一天測試啟動腳本
   - 確認所有服務正常運行
   - 記錄公開網址格式

2. **準備展示流程**：
   - 登入系統
   - 展示核心功能
   - 準備技術問題答案

3. **備份方案**：
   - 錄製展示影片（以防網路問題）
   - 準備截圖
   - 本地環境也要能運行

### 展示技巧

1. **開場**：
   - 簡單介紹專案背景（NGO 管理系統）
   - 說明技術棧（.NET 9 + React + SQL Server）

2. **展示順序**：
   - 先展示前端 UI（視覺化）
   - 再說明後端架構（技術細節）
   - 最後展示程式碼（如果有要求）

3. **亮點強調**：
   - Entity Framework 的複雜關聯設計
   - React 狀態管理
   - JWT 認證授權
   - Azure 整合（如果有）

---

**最後更新**：2025-11-01
**版本**：v2.0 (Cloudflare Tunnel)
**作者**：NGO Platform Team
