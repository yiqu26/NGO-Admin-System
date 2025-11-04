# NGO 平台本地開發環境 - 完整指南

## 🎉 配置完成！系統已成功啟動

---

## 📁 專案結構

```
C:\Users\lanli\source\repos\
├── NGO-User-Case\              # C# MVC 使用者前台
│   └── NGOPlatformWeb\         (http://localhost:5066)
├── NGO-Admin-System\           # React 社工後台管理系統
│   └── (React + Vite)          (http://localhost:5173)
└── NGO-Admin-System-WebAPI\    # C# WebAPI 後端 API
    └── (ASP.NET Core)          (http://localhost:5264)
```

**資料庫**：SQL Server (`YUNYUE\SQLEXPRESS`) - `NGOPlatformDb`

---

## 🚀 系統啟動

### 當前運行狀態：

| 服務 | 狀態 | 訪問地址 | 說明 |
|------|------|----------|------|
| **WebAPI 後端** | ✅ 運行中 | http://localhost:5264 | API 服務 |
| **React 後台** | ✅ 運行中 | http://localhost:5173 | 社工管理系統 |
| **C# MVC 前台** | ✅ 運行中 | http://localhost:5066 | 一般用戶平台 |

### 如何重新啟動系統

開啟三個終端機視窗，分別執行：

**終端機 1 - WebAPI 後端**（必須先啟動）：
```bash
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI
dotnet run
```

**終端機 2 - React 後台**：
```bash
cd C:\Users\lanli\source\repos\NGO-Admin-System
npm run dev
```

**終端機 3 - C# MVC 前台**（選用）：
```bash
cd C:\Users\lanli\source\repos\NGO-User-Case\NGOPlatformWeb
dotnet run
```

---

## 📚 重要文件說明

| 文件名稱 | 用途 | 位置 |
|---------|------|------|
| **本地開發環境啟動指南.md** | 詳細的啟動步驟和疑難排解 | `C:\Users\lanli\source\repos\` |
| **測試帳號與登入指南.md** | React 後台登入說明和測試帳號查詢 | `C:\Users\lanli\source\repos\` |
| **appsettings.json** | WebAPI 資料庫連線配置 | `NGO-Admin-System-WebAPI\` |
| **.env.development** | React 本地開發環境變數 | `NGO-Admin-System\` |

---

## 🔐 登入測試

### React 後台管理系統

**訪問地址**：http://localhost:5173

**登入方式**：
1. 資料庫登入（Email + 密碼）✅ 可用
2. Azure AD 登入 ❌ 本地環境已停用

**查看測試帳號**：
```sql
-- 在 SSMS 執行此查詢
SELECT Email, Name, Role
FROM Workers
WHERE IsActive = 1;
```

詳細說明請參考：`測試帳號與登入指南.md`

### C# MVC 使用者前台

**訪問地址**：http://localhost:5066

**說明**：
- 這是一般用戶使用的前台
- 用於瀏覽活動、報名、捐款等功能
- 與 React 後台使用同一個資料庫

---

## ⚙️ 配置文件說明

### 1. WebAPI 配置（appsettings.json）

**位置**：`NGO-Admin-System-WebAPI\appsettings.json`

**關鍵配置**：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YUNYUE\\SQLEXPRESS;Database=NGOPlatformDb;..."
  },
  "Jwt": {
    "Key": "本地開發用的JWT密鑰",
    ...
  }
}
```

**已配置**：
- ✅ 本地資料庫連線
- ✅ JWT 認證設定
- ✅ CORS 設定（允許 React 連接）

**未配置（功能受限）**：
- ❌ Azure Blob Storage（檔案上傳）
- ❌ Azure Speech（語音轉文字）
- ❌ Azure OpenAI（AI 輔助功能）

### 2. React 配置（.env.development）

**位置**：`NGO-Admin-System\.env.development`

**關鍵配置**：
```env
VITE_API_BASE_URL=http://localhost:5264/api
VITE_ENABLE_AZURE_LOGIN=false
```

**說明**：
- React 會自動連接本地 WebAPI
- Azure AD 登入已停用，使用資料庫登入

---

## 🔍 系統架構

```
┌─────────────┐
│  使用者瀏覽器  │
└──────┬──────┘
       │
       ├─────► React 後台 (localhost:5173)
       │         ↓ API 請求
       │       WebAPI 後端 (localhost:5264)
       │         ↓ SQL Query
       │       SQL Server 資料庫 (YUNYUE\SQLEXPRESS)
       │
       └─────► C# MVC 前台 (localhost:5066)
                 ↓ 直接連接
               SQL Server 資料庫 (同一個資料庫)
```

---

## ⚠️ 注意事項

### 啟動順序很重要
1. **先啟動 WebAPI** - React 後台需要連接 API
2. **再啟動 React** - 前端需要 API 才能正常運作
3. **最後啟動 C# MVC**（選用）

### 資料庫必須運行
確保 SQL Server 服務已啟動，可以在 SSMS 中測試連線。

### Azure 服務功能受限
以下功能在本地環境無法使用（需要 Azure 服務金鑰）：
- 📷 檔案/照片上傳（需要 Azure Blob Storage）
- 🎤 語音轉文字（需要 Azure Speech）
- 🤖 AI 輔助功能（需要 Azure OpenAI）

**其他功能正常**：
- ✅ 個案管理
- ✅ 活動管理
- ✅ 物資管理
- ✅ 帳號管理
- ✅ 儀表板統計

---

## 🧪 測試檢查清單

### 1. 基礎連線測試
- [ ] WebAPI 運行：訪問 http://localhost:5264 看到 "NGO API 運作正常"
- [ ] React 後台運行：訪問 http://localhost:5173 看到登入頁面
- [ ] C# MVC 前台運行：訪問 http://localhost:5066 看到首頁

### 2. 資料庫測試
- [ ] 在 SSMS 查詢 `SELECT * FROM Workers` 有資料
- [ ] WebAPI 可以連接資料庫（查看終端機無錯誤）

### 3. 登入功能測試
- [ ] React 後台可以輸入 Email 進行帳號驗證
- [ ] 成功登入後跳轉到 Dashboard

### 4. API 連線測試
- [ ] React 可以正常呼叫 WebAPI（檢查瀏覽器 Console 無 CORS 錯誤）
- [ ] 資料可以正常顯示（個案列表、活動列表等）

---

## 🚨 常見問題排除

### Q: WebAPI 無法啟動
**檢查**：
1. 資料庫連線字串是否正確？
2. SQL Server 是否運行？
3. Port 5264 是否被占用？

### Q: React 無法連接 API
**檢查**：
1. WebAPI 是否已啟動？
2. `.env.development` 中的 API 位址是否正確？
3. 瀏覽器 Console 是否有錯誤？

### Q: 登入失敗
**檢查**：
1. 資料庫中是否有測試帳號？
   ```sql
   SELECT Email, Name FROM Workers WHERE IsActive = 1;
   ```
2. 密碼是否正確？（詢問組員）
3. WebAPI 是否正常運行？

### Q: 找不到測試帳號
**解決方式**：
1. 查看 `測試帳號與登入指南.md`
2. 在資料庫中查詢現有帳號
3. 詢問組員測試帳號的密碼

---

## 📞 獲取幫助

遇到問題時，請提供以下資訊：

1. **錯誤訊息**：
   - 瀏覽器 Console 的錯誤
   - 終端機的錯誤訊息
   - SQL Server 的錯誤

2. **系統狀態**：
   - 哪個服務無法啟動？
   - 資料庫是否可以連線？

3. **測試結果**：
   - 已完成的測試項目
   - 失敗的測試項目

---

## 🎯 快速參考

### 啟動指令
```bash
# WebAPI
cd C:\Users\lanli\source\repos\NGO-Admin-System-WebAPI && dotnet run

# React
cd C:\Users\lanli\source\repos\NGO-Admin-System && npm run dev

# C# MVC
cd C:\Users\lanli\source\repos\NGO-User-Case\NGOPlatformWeb && dotnet run
```

### 訪問地址
- WebAPI: http://localhost:5264
- React 後台: http://localhost:5173
- C# MVC 前台: http://localhost:5066

### 查看測試帳號
```sql
SELECT Email, Name, Role FROM Workers WHERE IsActive = 1;
```

---

## 📖 相關文件

1. **本地開發環境啟動指南.md** - 詳細的啟動和配置說明
2. **測試帳號與登入指南.md** - 登入方式和測試帳號說明
3. **NGO-Admin-System/ARCHITECTURE.md** - React 專案架構文件
4. **NGO-Admin-System-WebAPI/README.md** - WebAPI 專案說明（如果有）

---

## ✨ 祝測試順利！

所有配置都已完成，系統已經可以正常運行。如果有任何問題，請參考相關文件或尋求協助。
