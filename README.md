# NGO 案件管理系統

> 完整的 NGO 案件管理、活動管理、物資配送與志工協作平台

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://reactjs.org/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

## 📋 專案簡介

這是一個為非營利組織設計的全功能管理系統，採用現代化前後端分離架構，提供完整的案件追蹤、活動管理、物資配送等功能。

### ✨ 核心功能

- 📊 **Dashboard 儀表板** - 即時統計與視覺化圖表
- 📝 **案件管理** - 完整的個案追蹤系統
- 🎯 **活動管理** - 活動建立、報名、追蹤
- 📦 **物資管理** - 庫存管理與配送追蹤
- 👥 **帳號管理** - 角色權限系統
- 📅 **行程管理** - 個人與團隊行程

### 🛠 技術棧

**後端**
- ASP.NET Core 9.0 (Web API)
- Entity Framework Core
- SQL Server

**前端 - 管理後台**
- React 18 + Vite
- Ant Design / Material-UI
- Chart.js

**前端 - 用戶前台**
- ASP.NET Core MVC (.NET 8)
- Google OAuth
- ECPay 金流

## 🚀 快速啟動

### 環境需求

- Node.js 18+
- .NET SDK 8.0 或 9.0
- SQL Server 2019+

### ⚡ 最快啟動方式

詳細步驟請參考：**[QUICK-START.md](QUICK-START.md)** 或 **[docs/00-快速開始.md](docs/00-快速開始.md)**

```bash
# 終端機 1 - 啟動後端 API
cd NGO-Management-System/api
dotnet run
# 運行於 http://localhost:5264

# 終端機 2 - 啟動 React 管理後台
cd NGO-Management-System/react-admin
npm run dev
# 運行於 http://localhost:5173

# 終端機 3 - 啟動 MVC 用戶前台（選用）
cd NGO-Management-System/dotnet-web/NGOPlatformWeb
dotnet run
# 運行於 http://localhost:5066
```

### 📚 完整文檔

- **[QUICK-START.md](QUICK-START.md)** - 3 分鐘快速啟動指南
- **[docs/00-快速開始.md](docs/00-快速開始.md)** - 詳細啟動步驟
- **[docs/02-Cloudflare展示指南.md](docs/02-Cloudflare展示指南.md)** - 公開展示設定
- **[docs/03-測試帳號.md](docs/03-測試帳號.md)** - 測試帳號與 ECPay 綠界資訊

## 🔐 測試帳號

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@ngo.org | Admin123! |
| 督導 | supervisor@ngo.org | Super123! |
| 員工 | staff@ngo.org | Staff123! |

## 📊 系統架構

```
     SQL Server
  (NGOPlatformDb)
         ↓
   後端 WebAPI
 ASP.NET Core 9
   (port 5264)
         ↓
    ┌────┴────┐
    ↓         ↓
React 前端    MVC 前端
管理後台      用戶前台
(5173)       (5066)
員工系統      民眾/個案
```

## 📁 專案結構

```
NGO-Management-System/
│
├── 📂 react-admin/             # React 管理後台（員工系統）
│   ├── src/                    # 原始碼
│   ├── public/                 # 靜態資源
│   ├── package.json
│   ├── vite.config.ts
│   └── README.md               # 前端說明文檔
│
├── 📂 api/                    # .NET 9 後端 API
│   ├── Controllers/            # API 控制器
│   ├── Models/                 # 資料模型
│   ├── Services/               # 業務邏輯
│   ├── appsettings.json
│   └── README.md               # 後端說明文檔
│
├── 📂 dotnet-web/             # MVC 用戶前台（民眾/個案系統）
│   ├── NGOPlatformWeb/         # MVC 專案
│   │   ├── Controllers/
│   │   ├── Views/
│   │   ├── Models/
│   │   └── appsettings.json
│   └── README.md               # MVC 說明文檔
│
├── 📂 docs/                    # 📚 完整文檔
│   ├── 00-快速開始.md
│   ├── 02-Cloudflare展示指南.md
│   ├── 03-測試帳號.md          # ⭐ 測試帳號與 ECPay 資訊
│   └── README.md
│
├── 📂 scripts/                 # 啟動腳本
│   └── 啟動展示系統-Cloudflare.bat
│
├── 📂 config/                  # 配置範例
│   └── README.md
│
├── 📂 database/                # 資料庫腳本
│   └── *.sql
│
├── 📄 README.md                # ⭐ 本文檔（專案總覽）
└── 📄 QUICK-START.md           # ⭐ 3 分鐘快速啟動
```

