# NGO 案件管理系統

> 非營利組織的完整數位化平台——從個案追蹤、活動報名到物資購買與 AI 輔助作業，涵蓋員工後台與公眾前台兩套系統。

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://reactjs.org/)
[![MVC](https://img.shields.io/badge/ASP.NET_MVC-.NET_8-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

## Live Demo

| 系統 | 網址 |
|------|------|
| 用戶前台（民眾） | https://ngo-management-hub.com |
| 員工管理後台 | https://admin.ngo-management-hub.com |
| Backend API | https://api.ngo-management-hub.com |

> Demo 透過 Cloudflare Tunnel 對外，需本機啟動服務才會連線。測試帳號見下方。

---

## 這個專案在做什麼

從一個小組協作專案發展而來，原始設計是讓 NGO 工作人員統一管理個案、活動與物資，同時提供公眾前台讓一般民眾報名活動、購買物資。

專案結束後我繼續獨自維護與擴充，補上當時來不及做的東西：AI 功能整合、ECPay 金流串接、UI 全站重設計，以及把一些比較粗糙的地方重構。

**我負責的主要範圍：**
- 資料庫設計與建置（SQL Server，含資料表設計、關聯規劃、View、Trigger）
- MVC 用戶前台的大部分開發（Controllers、Views、CSS、金流串接）
- 後期獨立開發：UI 重設計、ECPay 安全修正、AI 服務整合、持續重構

---

## 功能概覽

### 員工管理後台（React）

- **Dashboard** — 個案數、活動狀態、物資庫存的即時統計圖表（Recharts）
- **個案管理** — 新增/編輯個案，支援圖片上傳與**語音轉文字**（OpenAI Whisper）快速記錄案況
- **活動管理** — 建立活動、審核報名、用 **GPT-4o-mini 優化活動文案**、**DALL-E 3 生成活動封面圖**
- **物資管理** — 庫存追蹤、低庫存警示、常駐物資配對與緊急需求管理
- **帳號管理** — 三層角色權限（管理員 / 督導 / 員工），JWT 驗證

### 用戶前台（MVC）

- **Google OAuth** 第三方登入（自動建立帳號、頭像同步）
- **活動瀏覽與報名** — 即時名額檢查（DB Trigger 控制上限）
- **物資購物** — 加入購物車、結帳流程
- **ECPay 綠界金流** — 信用卡付款，SHA-256 CheckMacValue 雙重驗證
- **購買紀錄** — 訂單歷史查詢
- **成就系統** — 累積參與解鎖徽章，多表 LINQ 即時計算

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

- **React SPA**：員工後台需要大量互動操作（表格編輯、即時搜尋、圖表），SPA 體驗更好
- **MVC SSR**：用戶前台重視頁面載入速度與 SEO，Server-Side Rendering 更適合
- 兩套前端共用同一個 API，避免重複邏輯

---

## 技術設計亮點

### 1. 統一 API 回應格式

全站 API 統一使用 `ApiResponse<T>` wrapper：

```csharp
// 所有端點統一格式
return Ok(ApiResponse<IEnumerable<CaseDto>>.SuccessResponse(data, "查詢成功"));
return NotFound(ApiResponse<object>.ErrorResponse("找不到指定個案"));
```

前端 TypeScript service 層統一解包：
```typescript
const response = await api.get<{ data: Case[] }>('/Case');
return response.data;
```

### 2. 設計模式應用

| 模式 | 應用場景 | 說明 |
|------|---------|------|
| **Repository Pattern** | 個案資料存取 | `ICaseRepository` 分離 DB 操作與商業邏輯 |
| **Factory Pattern** | AI Client 建立 | `OpenAIClientFactory`（Singleton）依設定建立 OpenAI 或 Azure 的 Client |
| **Strategy Pattern** | 檔案儲存 | `IFileStorageService` 介面，Local / Azure Blob 兩種實作 |
| **三層架構** | 全系統 | Controller → Service → Repository，職責分離 |

### 3. AI 雙 Provider 設計

AI 服務同時支援 **OpenAI Direct** 和 **Azure OpenAI**，透過設定檔切換，不改程式碼：

```json
// appsettings.json
"AI": { "Provider": "OpenAI" }  // 改成 "Azure" 即切換
```

| 功能 | 模型 | 說明 |
|------|------|------|
| 文案優化 | GPT-4o-mini | 活動描述的 AI 改寫建議 |
| 封面圖生成 | DALL-E 3 | 根據活動內容生成圖片 |
| 語音轉文字 | Whisper-1 | 個案訪談記錄快速輸入 |

### 4. ECPay 金流安全設計

- **CheckMacValue 雙重驗證**：送出付款前產生 SHA-256 簽章，收到回調時再次驗證，防止偽造與重放攻擊
- **庫存扣減用 Transaction**：避免併發下的 Race Condition，確保不超賣
- 完整交易狀態機：Pending → Paid → Failed

### 5. 資安主動審查

專案做過一次 OWASP 風格的安全審查，修復的主要問題：

- 未授權的敏感端點（補上 `[Authorize(Roles)]`）
- `ViewBag` 的 Stack Trace 外洩（移除，改通用訊息）
- Hardcoded ID（改從 JWT Claims 取得）
- 密碼系統升級：明文 → BCrypt hash，零停機漸進式遷移
- ECPay 回調的 `Console.WriteLine` 印付款資料（移除）

---

## 本地啟動

### 需要的環境

- Node.js 18+
- .NET SDK 8 & 9
- SQL Server（本地或遠端）

### 啟動步驟

```bash
# 後端 API
cd api
dotnet run
# http://localhost:5264

# React 管理後台
cd react-admin
npm install
npm run dev
# http://localhost:5173

# MVC 用戶前台
cd dotnet-web/NGOPlatformWeb
dotnet run
# http://localhost:5066
```

`appsettings.json` 已被 `.gitignore` 排除，請複製 `api/appsettings.example.json` 並填入實際設定值。

---

## 測試帳號

### 員工後台

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@ngo.org | Admin123! |
| 督導 | supervisor@ngo.org | Super123! |
| 員工 | staff@ngo.org | Staff123! |

### 用戶前台

| Email | 密碼 |
|-------|------|
| test.user@example.com | Test123! |

### ECPay 測試信用卡
```
卡號：4311-9511-1111-1111　到期：12/25　CVV：222
```

---

## 專案結構

```
NGO-Management-System/
├── api/                         # ASP.NET Core 9 WebAPI
│   ├── Controllers/
│   │   ├── AccountManagement/   # 登入、帳號、工作人員
│   │   ├── ActivityManagement/  # 活動 CRUD、報名審核、AI 優化
│   │   ├── CaseManagement/      # 個案 CRUD、語音轉文字
│   │   ├── Dashboard/           # 統計數據
│   │   ├── ScheduleManagement/  # 行程管理
│   │   └── SupplyManagement/    # 物資、緊急需求、配送批次
│   ├── Models/
│   │   ├── Domain/              # 領域實體（按模組分資料夾）
│   │   ├── Infrastructure/      # NgoplatformDbContext
│   │   └── Shared/              # ApiResponse<T>
│   ├── Services/                # AI、語音、檔案儲存服務
│   ├── Repositories/            # CaseRepository（Repository Pattern）
│   ├── Validators/              # FluentValidation（個案 CRUD）
│   └── appsettings.example.json # 設定檔範本
│
├── react-admin/                 # React 18 員工後台
│   └── src/
│       ├── components/          # 各功能模組頁面
│       ├── services/            # API 服務層（統一 .data 解包）
│       └── hooks/               # useAuth 等
│
├── dotnet-web/NGOPlatformWeb/   # ASP.NET MVC 用戶前台
│   ├── Controllers/
│   ├── Views/
│   └── Services/                # ECPay、Email、成就系統
│
├── database/                    # SQL 建表腳本
├── docs/                        # 技術文件
└── scripts/                     # 一鍵啟動腳本
```

---

## 技術文件

| 文件 | 說明 |
|------|------|
| [系統架構與功能說明](docs/01-系統架構與功能說明.md) | 完整架構說明、各模組功能細節 |
| [ECPay 金流技術分析](docs/04-ECPay綠界金流技術分析與面試指南.md) | CheckMacValue 實作、安全設計說明 |
| [Cloudflare 展示指南](docs/02-Cloudflare展示指南.md) | Demo 環境啟動與 Tunnel 設定 |
