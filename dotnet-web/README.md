# NGO 平台前台系統

NGO 平台前台系統是一個基於 ASP.NET Core MVC 開發的公益平台，提供一般民眾與個案兩種角色的服務功能。系統整合了活動報名、物資認購、Google 登入、ECPay 金流等核心功能。

## 🚀 快速開始

### 系統需求
- .NET 8.0 或更高版本
- SQL Server 資料庫
- Visual Studio 2022 或 VS Code

### 安裝與執行
```bash
# 1. Clone 專案
git clone <repository-url>
cd NGO-User-Case

# 2. 安裝相依套件
dotnet restore

# 3. 設定資料庫連線字串（見下方 appsettings.json 設定說明）

# 4. 執行專案
dotnet run --project NGOPlatformWeb
```

## ⚙️ appsettings.json 設定說明

### 資料庫連線設定
```json
"ConnectionStrings": {
  "NGODb": "Server=tcp:ngosqlserver.database.windows.net,1433;Initial Catalog=NGOPlatformDb;..."
}
```
使用 Azure SQL Database，包含連線逾時與加密設定。

### Email 服務設定
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "NGO 平台"
}
```
用於密碼重設、通知信件寄送功能。

### Google OAuth 設定
```json
"Authentication": {
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

#### 🔧 Google OAuth 完整設定步驟

**1. 建立 Google Cloud 專案**
- 前往 [Google Cloud Console](https://console.cloud.google.com/)
- 點選「建立專案」或選擇現有專案
- 記下專案 ID

**2. 啟用 Google+ API**
- 在左側選單選擇「API 和服務」→「程式庫」
- 搜尋「Google+ API」並啟用
- 同時啟用「People API」（用於取得使用者資訊）

**3. 設定 OAuth 同意畫面**
- 選擇「API 和服務」→「OAuth 同意畫面」
- 選擇「外部」用戶類型
- 填寫必要資訊：
  - 應用程式名稱：`NGO 平台`
  - 使用者支援電子郵件：開發者的 Gmail
  - 授權網域：`localhost`（開發用）
  - 開發人員聯絡資訊：開發者的 Gmail

**4. 建立 OAuth 2.0 憑證**
- 選擇「API 和服務」→「憑證」
- 點選「建立憑證」→「OAuth 2.0 用戶端 ID」
- 應用程式類型選擇「網路應用程式」
- 設定重新導向 URI：
  ```
  http://localhost:5000/signin-google
  https://localhost:5001/signin-google
  ```
- 複製產生的 `Client ID` 和 `Client Secret`

> 💡 **使用 Ngrok 時的額外設定**
> 如需在 Ngrok 環境下測試 Google 登入，還需要將 Ngrok URL 加入重新導向 URI：
> ```
> https://your-ngrok-url.ngrok-free.app/signin-google
> ```
> 注意：每次 Ngrok URL 改變時都需要更新此設定！

**5. 更新 appsettings.json**
```json
"Authentication": {
  "Google": {
    "ClientId": "123456789-abcdefghijk.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-your-actual-secret-key"
  }
}
```

### ECPay 金流設定
```json
"ECPay": {
  "UseNgrok": true,
  "NgrokUrl": "https://your-ngrok-url.ngrok-free.app",
  "Comment": "暫時停用 Ngrok 以測試 Google 登入，ECPay 功能需要 Ngrok"
}
```

#### 🌐 Ngrok 完整設定步驟

**1. 安裝 Ngrok**
```bash
# 方法一：使用 Chocolatey (Windows)
choco install ngrok

# 方法二：直接下載
# 前往 https://ngrok.com/download 下載對應版本
```

**2. 註冊 Ngrok 帳號**
- 前往 [Ngrok 官網](https://ngrok.com/) 註冊免費帳號
- 登入後到 Dashboard 取得 AuthToken

**3. 設定 AuthToken**
```bash
ngrok config add-authtoken your-authtoken-here
```

**4. 啟動 Ngrok 隧道**
```bash
# 假設應用程式運行在 localhost:5000
ngrok http 5000

# 或指定特定域名（付費功能）
ngrok http 5000 --domain=your-reserved-domain.ngrok-free.app
```

**5. 複製 Ngrok URL**
- Ngrok 啟動後會顯示類似：
  ```
  Forwarding    https://abc123def456.ngrok-free.app -> http://localhost:5000
  ```
- 複製 HTTPS 網址到 appsettings.json

**6. 更新 ECPay 設定**
```json
"ECPay": {
  "UseNgrok": true,
  "NgrokUrl": "https://abc123def456.ngrok-free.app"
}
```

#### ⚠️ 重要提醒

**Google OAuth 注意事項：**
- 測試時確保 `ECPay.UseNgrok` 設為 `false`，避免衝突
- 本地開發建議使用 `http://localhost:5000`
- 正式環境需要更新 OAuth 重新導向 URI

**Ngrok 注意事項：**
- 免費版 Ngrok URL 每次重啟會變動
- 付費版可保留固定域名
- ECPay 回呼需要公開網址，因此本地開發必須使用 Ngrok

**開發流程建議：**
```bash
# 1. 測試 Google 登入時（僅本地）
# appsettings.json -> "UseNgrok": false
dotnet run
# 使用 http://localhost:5000 測試

# 2. 測試 ECPay 付款時
# appsettings.json -> "UseNgrok": true
ngrok http 5000
# 更新 NgrokUrl 後重啟應用程式

# 3. 同時測試 Google 登入 + ECPay 時
ngrok http 5000
# 1) 複製 Ngrok URL 到 appsettings.json
# 2) 到 Google Cloud Console 新增 Ngrok URL 到重新導向 URI
# 3) 重啟應用程式
```

#### 🔄 Ngrok URL 變更時的處理流程

當 Ngrok URL 改變時（重啟 Ngrok 或免費版 URL 輪換），需要執行：

1. **更新 appsettings.json**
   ```json
   "NgrokUrl": "https://new-ngrok-url.ngrok-free.app"
   ```

2. **更新 Google OAuth 設定**
   - 前往 Google Cloud Console → API 和服務 → 憑證
   - 編輯 OAuth 2.0 用戶端 ID
   - 移除舊的 Ngrok URL，新增新的 URL：
     ```
     https://new-ngrok-url.ngrok-free.app/signin-google
     ```

3. **重啟應用程式**
   ```bash
   dotnet run
   ```

### Token 清理設定
```json
"TokenCleanup": {
  "CleanupIntervalHours": 24,
  "RetentionDays": 7
}
```
自動清理過期的密碼重設 Token。

## 🏗️ 專案架構

### 主要控制器
- **HomeController**: 首頁、關於我們、聯絡我們
- **AuthController**: 登入、註冊、密碼重設
- **UserController**: 一般民眾功能（個人資料、購買紀錄、活動報名）
- **CaseController**: 個案專屬功能（活動清單、物資認購）
- **ActivityController**: 活動相關功能
- **PurchaseController**: 物資購買、ECPay 金流處理

### 核心功能
- 🔐 多重身份驗證（本地帳號、Google OAuth）
- 📅 活動報名系統
- 🛒 物資認購與線上付款
- 👤 個人資料管理與成就系統
- 📧 Email 通知服務
- 💳 ECPay 金流整合

## 🗄️ 資料庫結構

系統包含以下主要資料表：
- **Users/Cases**: 使用者與個案資料
- **Activities**: 活動管理
- **Supplies**: 物資管理
- **Orders**: 訂單與交易紀錄
- **Achievements**: 使用者成就系統

## 🌐 部署架構

- **分支管理**: `main` (正式版) / `develop` (開發版)
- **資料庫**: Azure SQL Database
- **檔案儲存**: 本地檔案系統 (wwwroot/images)
- **容器化**: 支援 Docker 部署
