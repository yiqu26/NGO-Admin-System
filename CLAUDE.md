# NGO Management System - Claude 工作記錄

> 最後更新: 2026-02-07

## 專案概述

**NGO 案件管理系統** - 完整的非營利組織管理平台，包含員工後台與用戶前台。

| 組件 | 技術 | 用途 |
|------|------|------|
| Backend | ASP.NET Core 9 WebAPI | 統一 API 服務 |
| Frontend (React) | React 18 + Vite + MUI | 員工管理後台 |
| Frontend (MVC) | ASP.NET Core MVC .NET 8 | 用戶前台 |
| Database | SQL Server | 資料儲存 |

---

## 本地開發

### 端口配置

| 服務 | 本地 URL |
|------|----------|
| Backend API | http://localhost:5264 |
| React Admin | http://localhost:5173 |
| MVC Portal | http://localhost:5066 |

### 啟動方式

```bash
# 一鍵啟動 (含 Cloudflare Tunnel)
scripts\start-demo-smart.bat

# 或手動啟動
cd api && dotnet run                         # :5264
cd react-admin && npm run dev                 # :5173
cd dotnet-web/NGOPlatformWeb && dotnet run   # :5066
```

### 測試帳號

| 系統 | 帳號 | 密碼 |
|------|------|------|
| React Admin | admin@ngo.org | Admin123! |
| MVC Portal | test.user@example.com | Test123! |

### ECPay 測試卡號
```
卡號: 4311-9511-1111-1111
到期: 12/25
CVV: 222
```

---

## 公開展示 (Cloudflare Tunnel)

| 服務 | 固定網域 |
|------|----------|
| MVC 用戶前台 | https://ngo-management-hub.com |
| React 管理後台 | https://admin.ngo-management-hub.com |
| Backend API | https://api.ngo-management-hub.com |

啟動腳本會自動建立 Cloudflare Tunnel。

---

## 專案結構

```
NGO-Management-System/
├── api/                         # ASP.NET Core 9 WebAPI
│   ├── Controllers/
│   │   ├── AccountManagement/    # 登入、帳號、工作人員
│   │   ├── ActivityManagement/   # 活動、報名審核、AI 優化
│   │   ├── CaseManagement/       # 個案 CRUD、語音轉文字
│   │   ├── Dashboard/            # 統計數據
│   │   ├── ScheduleManagement/   # 行程管理
│   │   └── SupplyManagement/     # 物資、緊急需求、配送
│   ├── Models/
│   ├── Services/
│   │   ├── OpenAIClientFactory.cs  # AI client 統一工廠 (OpenAI/Azure 雙 provider)
│   │   ├── AzureOpenAIService.cs   # GPT 文案優化
│   │   ├── WhisperService.cs       # OpenAI Whisper 語音轉文字
│   │   └── FileStorageService.cs   # 檔案儲存 (Local/Azure Blob)
│   └── appsettings.json
│
├── react-admin/                  # React 管理後台
│   ├── src/
│   │   ├── components/           # 各功能模組
│   │   ├── pages/                # Dashboard, Cases, Activities...
│   │   ├── services/             # API 服務層
│   │   └── contexts/             # AuthProvider
│   ├── .env.development
│   └── .env.cloudflare
│
├── dotnet-web/NGOPlatformWeb/    # MVC 用戶前台
│   ├── Controllers/              # Home, Auth, User, Case, Purchase
│   ├── Views/
│   ├── Services/                 # ECPay, Email, Achievement
│   └── wwwroot/
│
├── docs/                         # 完整文檔 (5600+ 行)
│   ├── 00-快速開始.md
│   ├── 01-系統架構與功能說明.md
│   ├── 02-Cloudflare展示指南.md
│   ├── 03-測試帳號.md
│   └── 04-ECPay綠界金流技術分析.md
│
├── scripts/
│   └── start-demo-smart.bat      # 一鍵啟動
│
└── cloudflare-urls.txt           # 固定網址記錄
```

---

## 已完成功能

### 員工後台 (React)
- [x] JWT 身份驗證
- [x] Dashboard 統計圖表
- [x] 個案管理 (CRUD、搜尋、圖片上傳)
- [x] 活動管理 (CRUD、報名審核)
- [x] 物資管理 (庫存、低庫存警示)
- [x] 行程管理
- [x] 帳號權限管理 (管理員/督導/員工)
- [x] 語音轉文字 (OpenAI Whisper / Azure Speech 雙 provider)
- [x] AI 活動優化 (OpenAI Direct API / Azure OpenAI 雙 provider)
- [x] AI 圖片生成 (DALL-E 3，支援 OpenAI / Azure 雙 provider)

### 用戶前台 (MVC)
- [x] Google OAuth 登入
- [x] 活動報名
- [x] 物資購物與下單
- [x] ECPay 金流支付
- [x] 購買紀錄
- [x] 個人資料編輯
- [x] 成就系統

---

## AI 功能架構 (2026-02-07 更新)

### Provider 設定

AI 功能支援 **OpenAI Direct API** 和 **Azure OpenAI** 雙 provider，透過 `appsettings.json` 的 `AI:Provider` 切換：

| Provider | 設定值 | 需要填入 |
|----------|--------|---------|
| **OpenAI (預設)** | `"OpenAI"` | `AI:OpenAI:ApiKey` 一個值即可啟用全部功能 |
| Azure | `"Azure"` | `AI:Azure:Endpoint` + `AI:Azure:ApiKey` |

### 三項 AI 功能

| 功能 | OpenAI 模型 | Azure 模型 | API Endpoint |
|------|------------|------------|-------------|
| GPT 文案優化 | gpt-4o-mini | gpt-4.1 (deployment) | `POST /api/ActivityAIOptimizer/optimize-description` |
| DALL-E 圖片生成 | dall-e-3 | dall-e-3 (deployment) | `POST /api/ActivityImageGenerator/generate` |
| Whisper 語音轉文字 | whisper-1 | Azure Speech SDK | `POST /api/CaseSpeechToText/transcribe` |

### 關鍵服務

| Service | DI 生命週期 | 說明 |
|---------|-----------|------|
| `OpenAIClientFactory` | Singleton | 統一建立 OpenAIClient，讀取 AI:Provider 決定 provider |
| `AzureOpenAIService` | Scoped | GPT 文案優化，透過 factory 取得 client |
| `WhisperService` | Scoped | 透過 HttpClient 呼叫 OpenAI Whisper API |
| `IFileStorageService` | Scoped | Local (預設) 或 Azure Blob，音檔儲存到 `uploads/` |

### 檔案儲存

`FileStorage:Provider` 設定為 `"Local"` 時，音檔存到 `api/uploads/case_audio/`，透過靜態檔案中介軟體提供 `/uploads` URL 存取。不需要 Azure Blob Storage。

### 狀態確認 API

- `GET /api/ActivityAIOptimizer/status` → `isAvailable: true/false`
- `POST /api/ActivityImageGenerator/test-connection` → `success: true/false`
- `GET /api/CaseSpeechToText/test-connection` → 顯示 provider 名稱

### 注意事項

- `appsettings.json` 已在 `.gitignore`，API Key 不會進 git
- 舊版 `AzureOpenAI`、`AzureSpeech` config 區段保留向下相容
- `Azure.AI.OpenAI` v1.0.0-beta.13 同時支援 OpenAI Direct 和 Azure，不需額外套件

---

## 外部服務

| 服務 | 用途 | 配置位置 |
|------|------|----------|
| OpenAI API | GPT-4o-mini、DALL-E 3、Whisper | api/appsettings.json `AI:OpenAI` |
| Azure OpenAI (備選) | GPT-4、DALL-E 3 | api/appsettings.json `AI:Azure` |
| Azure Speech (備選) | 語音轉文字 | api/appsettings.json `AI:AzureSpeech` |
| Azure Blob (備選) | 圖片/音檔儲存 | api/appsettings.json `FileStorage` |
| Google OAuth | 第三方登入 | dotnet-web/NGOPlatformWeb/appsettings.json |
| ECPay | 金流支付 | dotnet-web/NGOPlatformWeb/appsettings.json |
| Cloudflare Tunnel | 公開展示 | 本機 cloudflared |

---

## 注意事項

1. **Cloudflare Tunnel**: 使用 Named Tunnel `ngo-demo`，固定網域無需每次重設
2. **環境變數**: `.env.cloudflare` 用於公開展示，`.env.development` 用於本地開發
3. **資料庫**: 使用本地 SQL Server (YUNYUE\SQLEXPRESS)
4. **文檔**: `docs/` 目錄有完整的技術文檔
5. **API Key 安全**: `appsettings.json` 已被 `.gitignore` 排除，敏感資訊不會進版控
