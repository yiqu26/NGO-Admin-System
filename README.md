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
- Cloudflared (用於公開展示，選用)

### 本地開發啟動

```bash
# 1. Clone 專案
git clone https://github.com/your-username/NGO-Admin-System.git
cd NGO-Admin-System

# 2. 安裝後端依賴
cd NGO-Admin-System-WebAPI
dotnet restore && dotnet build

# 3. 安裝前端依賴
cd ../NGO-Admin-System
npm install

# 4. 啟動後端 API (終端機 1)
cd NGO-Admin-System-WebAPI
dotnet run
# 運行於 http://localhost:5264

# 5. 啟動前端 (終端機 2)
cd NGO-Admin-System
npm run dev
# 運行於 http://localhost:5173
```

### 🌐 公開展示（使用 Cloudflare Tunnel）

如需透過公開網址展示系統（例如給面試官），可使用啟動腳本：

```bash
# 使用一鍵啟動腳本
scripts\啟動展示系統-Cloudflare.bat
```

這會自動啟動：
- 後端 API + React 前端 + MVC 前端
- 3 個 Cloudflare Tunnel（自動產生公開網址）

詳細說明請參考 [CLOUDFLARE-TUNNEL-GUIDE.md](docs/CLOUDFLARE-TUNNEL-GUIDE.md)

---

詳細安裝步驟請參考 [QUICK-START.md](QUICK-START.md)

## 🔐 測試帳號

| 角色 | Email | 密碼 |
|------|-------|------|
| 管理員 | admin@ngo.org | Admin123! |
| 督導 | supervisor@ngo.org | Super123! |
| 員工 | staff@ngo.org | Staff123! |

## 📊 系統架構

```
SQL Server (NGOPlatformDb)
         ↓
    後端 API (5264)
    ASP.NET Core 9
         ↓
    ┌────┴────┐
    ↓         ↓
React 前端  MVC 前端
(5173)     (5066)
管理後台    用戶前台
```

## 📁 專案結構

```
├── NGO-Admin-System/           # React 管理後台
├── NGO-Admin-System-WebAPI/    # .NET 9 後端 API
├── NGO-User-Case/              # MVC 用戶前台
├── scripts/                    # 啟動腳本
├── docs/                       # 文檔
└── README.md
```

## 📸 系統截圖

### Dashboard 儀表板
[待補充]

### 案件管理
[待補充]

## 🤝 貢獻

歡迎提交 Issue 或 Pull Request！

## 📄 授權

MIT License

## 👨‍💻 開發者

- **GitHub**: [你的 GitHub]
- **Email**: [你的 Email]

---

**⭐ 如果喜歡這個專案，請給個 Star！**

> 詳細文檔請參考 [README-PROFESSIONAL.md](README-PROFESSIONAL.md)
