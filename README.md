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

> Demo 使用 Cloudflare Tunnel 對外，服務需要本機啟動才會連線。測試帳號見下方。

---

## 這個專案在做什麼

培訓機構的小組專案，原始設計是讓 NGO 的工作人員能統一管理個案、活動與物資，同時提供一個公眾前台讓一般民眾報名活動、購買物資。

結訓之後我把它當作個人練習的延伸，陸續補上當時來不及做的東西：AI 功能整合、ECPay 金流串接、UI 全站重設計、以及把一些當時趕出來比較粗糙的地方重構。

**我在這個專案的主要負責範圍：**
- 資料庫設計與建置（SQL Server，含資料表設計、關聯規劃）
- MVC 用戶前台的大部分開發（Controllers、Views、CSS、金流串接）
- 結訓後：UI 重設計、ECPay 安全修正、AI 服務整合、持續重構

---

## 功能概覽

### 員工管理後台（React）

- **Dashboard** - 個案數、活動狀態、物資庫存的即時統計圖表
- **個案管理** - 新增/編輯個案，支援圖片上傳與**語音轉文字**（OpenAI Whisper）記錄案況
- **活動管理** - 建立活動、審核報名、用 **GPT-4o-mini 優化活動文案**、**DALL-E 3 生成活動封面圖**
- **物資管理** - 庫存追蹤、低庫存警示
- **帳號管理** - 三層角色權限（管理員 / 督導 / 員工），JWT 驗證

### 用戶前台（MVC）

- **Google OAuth** 第三方登入
- **活動瀏覽與報名** - 一般民眾可報名 NGO 活動
- **物資購物** - 加入購物車、結帳流程
- **ECPay 綠界金流** - 信用卡付款（含 SHA-256 checksum 驗證）
- **購買紀錄** - 訂單歷史查詢
- **成就系統** - 累積參與解鎖徽章

---

## 技術架構

```
┌─────────────────────────────────┐
│         SQL Server              │
│       (NGOPlatformDb)           │
└──────────────┬──────────────────┘
               │
┌──────────────▼──────────────────┐
│      ASP.NET Core 9 WebAPI      │
│         (port 5264)             │
│  Controllers / Services / EF    │
└───────┬──────────────┬──────────┘
        │              │
┌───────▼──────┐ ┌─────▼────────────┐
│  React 18    │ │  ASP.NET MVC     │
│  + Vite      │ │  .NET 8          │
│  (port 5173) │ │  (port 5066)     │
│              │ │                  │
│  員工後台    │ │  用戶前台        │
│  JWT Auth    │ │  Google OAuth    │
│  MUI + MUI X │ │  ECPay 金流      │
└──────────────┘ └──────────────────┘
```

### AI 功能的設計方式

AI 服務同時支援 **OpenAI Direct** 和 **Azure OpenAI** 兩個 provider，透過 `appsettings.json` 的 `AI:Provider` 切換，不需要改程式碼。這樣設計是因為 Azure OpenAI 有些部署限制，直接用 OpenAI API 開發期間比較方便，但實際部署可能需要 Azure 版本。

| 功能 | 模型 | 說明 |
|------|------|------|
| 文案優化 | GPT-4o-mini | 活動描述的 AI 改寫建議 |
| 封面圖生成 | DALL-E 3 | 根據活動內容生成圖片 |
| 語音轉文字 | Whisper-1 | 個案訪談記錄快速輸入 |

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

`appsettings.json` 已被 `.gitignore` 排除（含 DB 連線字串、API Key），請自行建立並填入對應設定。範例結構在 `docs/` 裡。

---

## 測試帳號

**員工後台**

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@ngo.org | Admin123! |
| 督導 | supervisor@ngo.org | Super123! |
| 員工 | staff@ngo.org | Staff123! |

**用戶前台**

| Email | 密碼 |
|-------|------|
| test.user@example.com | Test123! |

**ECPay 測試信用卡**
```
卡號：4311-9511-1111-1111　到期：12/25　CVV：222
```

---

## 專案結構

```
NGO-Management-System/
├── api/                    # ASP.NET Core 9 WebAPI
│   ├── Controllers/
│   ├── Models/
│   └── Services/           # AI、語音、檔案儲存服務
├── react-admin/            # React 18 員工後台
│   └── src/
│       ├── components/
│       ├── pages/
│       └── services/
├── dotnet-web/             # ASP.NET MVC 用戶前台
│   └── NGOPlatformWeb/
│       ├── Controllers/
│       ├── Views/
│       └── Services/       # ECPay、Email、成就系統
├── database/               # SQL 建表腳本
├── docs/                   # 詳細技術文件
└── scripts/                # 一鍵啟動腳本
```
