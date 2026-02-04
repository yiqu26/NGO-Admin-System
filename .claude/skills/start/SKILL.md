---
name: start
description: 啟動 NGO-Management-System 本地開發環境，顯示端口和測試帳號資訊
disable-model-invocation: true
user-invocable: true
---

# NGO Management System 開發環境啟動

## 服務端口

| 服務 | URL | 說明 |
|------|-----|------|
| Backend API | http://localhost:5264 | ASP.NET Core 9 WebAPI |
| React Admin | http://localhost:5173 | 員工管理後台 |
| MVC Portal | http://localhost:5066 | 用戶前台 |

## 啟動方式

### 方法 1: 使用啟動腳本 (含 Cloudflare)
```bash
scripts\start-demo-smart.bat
```

### 方法 2: 手動啟動 (開發用)
```bash
# Terminal 1 - 後端 API
cd backend && dotnet run

# Terminal 2 - React 管理後台
cd frontend && npm run dev

# Terminal 3 - MVC 用戶前台
cd mvc-frontend/NGOPlatformWeb && dotnet run
```

## 測試帳號

| 系統 | 帳號 | 密碼 |
|------|------|------|
| React Admin | admin@ngo.org | Admin123! |
| MVC Portal | test.user@example.com | Test123! |

### ECPay 測試卡號
- 卡號: `4311-9511-1111-1111`
- 到期: `12/25`
- CVV: `222`

## Cloudflare 固定網域

| 服務 | 網址 |
|------|------|
| MVC 用戶前台 | https://ngo-management-hub.com |
| React 管理後台 | https://admin.ngo-management-hub.com |
| Backend API | https://api.ngo-management-hub.com |

## 技術棧速覽

| 組件 | 技術 |
|------|------|
| Backend | ASP.NET Core 9 + EF Core + JWT |
| React Frontend | React 18 + Vite + MUI |
| MVC Frontend | ASP.NET Core MVC .NET 8 |
| Database | SQL Server (YUNYUE\SQLEXPRESS) |
| AI | Azure OpenAI (GPT-4, DALL-E 3) |
| 金流 | ECPay 綠界 |

## 注意事項
- React 用 `.env.development` (localhost)，展示用 `.env.cloudflare`
- 資料庫是本地 SQL Server，非 Docker
- 完整文檔在 `docs/` 目錄
