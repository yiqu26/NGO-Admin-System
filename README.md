# NGO 案件管理系統

非營利組織的後台管理平台，涵蓋個案追蹤、活動報名、物資管理與 AI 輔助功能，分為員工後台與公眾前台兩套系統。

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://reactjs.org/)
[![MVC](https://img.shields.io/badge/ASP.NET_MVC-.NET_8-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

> 個人作品集專案。

---

## Live Demo

| 系統 | 網址 |
|------|------|
| 用戶前台（民眾） | https://ngo-management-hub.com |
| 員工管理後台 | https://admin.ngo-management-hub.com |
| Backend API | https://api.ngo-management-hub.com |

> Demo 透過 Cloudflare Tunnel 對外，需本機啟動服務才會連線。測試帳號見下方。

---

## 功能概覽

### 員工管理後台（React）

- **Dashboard** — 個案數、活動狀態、物資庫存的即時統計圖表（Recharts）
- **個案管理** — 新增/編輯個案，支援圖片上傳與語音轉文字（OpenAI Whisper）快速記錄案況
- **活動管理** — 建立活動、審核報名、GPT-4o-mini 文案改寫建議、DALL-E 3 生成封面圖
- **物資管理** — 庫存追蹤、低庫存警示、常駐物資配對與緊急需求管理
- **帳號管理** — 三層角色權限（管理員 / 督導 / 員工），JWT 驗證

### 用戶前台（MVC）

- **Google OAuth** 第三方登入（自動建立帳號、頭像同步）
- **活動瀏覽與報名** — 即時名額檢查（DB Trigger 控制上限）
- **物資購物** — 瀏覽、選購、結帳流程
- **ECPay 綠界金流** — 信用卡付款，SHA-256 CheckMacValue 雙向驗證
- **購買紀錄** — 訂單歷史查詢
- **成就系統** — 累積參與解鎖徽章

---

## 系統架構

```
┌─────────────────────────────────┐
│         SQL Server              │
│       (NGOPlatformDb)           │
└──────────────┬──────────────────┘
               │ EF Core (Database-First)
┌──────────────▼──────────────────┐
│      ASP.NET Core 9 WebAPI      │
│         (port 5264)             │
│  Controllers / Services / EF    │
└───────┬──────────────┬──────────┘
        │ JWT Auth     │ Cookie Auth
┌───────▼──────┐ ┌─────▼────────────┐
│  React 18    │ │  ASP.NET MVC     │
│  + Vite      │ │  .NET 8          │
│  (port 5173) │ │  (port 5066)     │
│  員工後台    │ │  用戶前台        │
│  MUI + TS    │ │  ECPay 金流      │
└──────────────┘ └──────────────────┘
```

### 為什麼前端做兩套？

- **React SPA**：員工後台操作密集（表格編輯、即時搜尋、圖表），SPA 互動體驗較好
- **MVC SSR**：用戶前台以瀏覽為主，Server-Side Rendering 首屏速度較快且對 SEO 友善
- 兩套前端共用同一個 API，避免重複邏輯

---

## 技術實作說明

### 統一 API 回應格式

全站 API 使用 `ApiResponse<T>` 統一包裝回應，前端不需各自處理不同的回應結構：

```csharp
return Ok(ApiResponse<IEnumerable<CaseDto>>.SuccessResponse(data, "查詢成功"));
return NotFound(ApiResponse<object>.ErrorResponse("找不到指定個案"));
```

```typescript
const response = await api.get<{ data: Case[] }>('/Case');
return response.data;
```

### 設計模式

| 模式 | 應用場景 | 說明 |
|------|---------|------|
| **Repository Pattern** | 個案資料存取 | `ICaseRepository` 分離 DB 操作與商業邏輯 |
| **Factory Pattern** | AI Client 建立 | `OpenAIClientFactory`（Singleton）依設定建立 OpenAI 或 Azure Client |
| **Strategy Pattern** | 檔案儲存 | `IFileStorageService` 介面，Local / Azure Blob 兩種實作可切換 |

### AI 雙 Provider

同時支援 OpenAI Direct 和 Azure OpenAI，透過設定檔切換，不需修改程式碼：

```json
"AI": { "Provider": "OpenAI" }
```

| 功能 | 模型 |
|------|------|
| 文案改寫建議 | GPT-4o-mini |
| 封面圖生成 | DALL-E 3 |
| 語音轉文字 | Whisper-1 |

### ECPay 金流

- 送出付款前產生 SHA-256 CheckMacValue，收到回調時再次驗證，防止偽造
- 庫存扣減在 ECPay 回調確認後才執行，避免付款失敗仍扣庫存
- 交易狀態：Pending → Paid / Failed

### 安全性

對照 OWASP Top 10 自行進行安全檢視，主要修正項目：

- 補上缺漏的 `[Authorize(Roles)]`，限制未授權存取敏感端點
- 移除 `ViewBag` 的 Stack Trace 輸出，改回通用錯誤訊息
- 識別碼改從 JWT Claims 取得，移除 Hardcoded ID
- 密碼從明文升級為 BCrypt hash，採漸進式遷移不中斷服務
- 移除 ECPay 回調中的付款資料 log

---

## 本地啟動

**環境需求**：Node.js 18+、.NET SDK 8 & 9、SQL Server

```bash
# 後端 API
cd api && dotnet run
# http://localhost:5264

# React 管理後台
cd react-admin && npm install && npm run dev
# http://localhost:5173

# MVC 用戶前台
cd dotnet-web/NGOPlatformWeb && dotnet run
# http://localhost:5066
```

`appsettings.json` 已被 `.gitignore` 排除，請複製 `api/appsettings.example.json` 並填入實際設定值。

---

## 測試帳號

員工後台與用戶前台各有預設測試帳號（管理員、督導、員工、一般用戶、個案），本地啟動後即可登入。ECPay 使用綠界官方測試卡號。

---

## 專案結構

```
NGO-Management-System/
├── api/                         # ASP.NET Core 9 WebAPI
│   ├── Controllers/
│   │   ├── AccountManagement/   # 登入、帳號、工作人員
│   │   ├── ActivityManagement/  # 活動 CRUD、報名審核、AI 功能
│   │   ├── CaseManagement/      # 個案 CRUD、語音轉文字
│   │   ├── Dashboard/           # 統計數據
│   │   ├── ScheduleManagement/  # 行程管理
│   │   └── SupplyManagement/    # 物資、緊急需求、配送批次
│   ├── Models/
│   │   ├── Domain/              # 領域實體（按模組分資料夾）
│   │   ├── Infrastructure/      # NgoplatformDbContext
│   │   └── Shared/              # ApiResponse<T>
│   ├── Services/                # AI、語音、檔案儲存服務
│   ├── Repositories/            # CaseRepository
│   ├── Validators/              # FluentValidation（個案 CRUD）
│   └── appsettings.example.json
│
├── react-admin/                 # React 18 員工後台
│   └── src/
│       ├── components/          # 各功能模組頁面
│       ├── services/            # API 服務層
│       └── hooks/
│
├── dotnet-web/NGOPlatformWeb/   # ASP.NET MVC 用戶前台
│   ├── Controllers/
│   ├── Views/
│   └── Services/                # ECPay、Email、成就系統
│
├── database/                    # SQL 建表腳本
└── scripts/                     # 一鍵啟動腳本
```
