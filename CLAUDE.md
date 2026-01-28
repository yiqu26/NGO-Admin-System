# NGO Management System - Claude 工作記錄

> 最後更新: 2026-01-28

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
cd backend && dotnet run                      # :5264
cd frontend && npm run dev                    # :5173
cd mvc-frontend/NGOPlatformWeb && dotnet run  # :5066
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
├── backend/                      # ASP.NET Core 9 WebAPI
│   ├── Controllers/
│   │   ├── AccountManagement/    # 登入、帳號、工作人員
│   │   ├── ActivityManagement/   # 活動、報名審核、AI 優化
│   │   ├── CaseManagement/       # 個案 CRUD、語音轉文字
│   │   ├── Dashboard/            # 統計數據
│   │   ├── ScheduleManagement/   # 行程管理
│   │   └── SupplyManagement/     # 物資、緊急需求、配送
│   ├── Models/
│   ├── Services/
│   └── appsettings.json
│
├── frontend/                     # React 管理後台
│   ├── src/
│   │   ├── components/           # 各功能模組
│   │   ├── pages/                # Dashboard, Cases, Activities...
│   │   ├── services/             # API 服務層
│   │   └── contexts/             # AuthProvider
│   ├── .env.development
│   └── .env.cloudflare
│
├── mvc-frontend/NGOPlatformWeb/  # MVC 用戶前台
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
- [x] 語音轉文字 (Azure Speech)
- [x] AI 活動優化 (Azure OpenAI)
- [x] AI 圖片生成 (DALL-E 3)

### 用戶前台 (MVC)
- [x] Google OAuth 登入
- [x] 活動報名
- [x] 物資購物與下單
- [x] ECPay 金流支付
- [x] 購買紀錄
- [x] 個人資料編輯
- [x] 成就系統

---

## 外部服務

| 服務 | 用途 | 配置位置 |
|------|------|----------|
| Azure OpenAI | GPT-4、DALL-E 3 | backend/appsettings.json |
| Azure Speech | 語音轉文字 | backend/appsettings.json |
| Azure Blob | 圖片儲存 | backend/appsettings.json |
| Google OAuth | 第三方登入 | mvc-frontend/appsettings.json |
| ECPay | 金流支付 | mvc-frontend/appsettings.json |
| Cloudflare Tunnel | 公開展示 | 本機 cloudflared |

---

## 注意事項

1. **Cloudflare Tunnel**: 使用 Named Tunnel `ngo-demo`，固定網域無需每次重設
2. **環境變數**: `.env.cloudflare` 用於公開展示，`.env.development` 用於本地開發
3. **資料庫**: 使用本地 SQL Server (YUNYUE\SQLEXPRESS)
4. **文檔**: `docs/` 目錄有完整的技術文檔
