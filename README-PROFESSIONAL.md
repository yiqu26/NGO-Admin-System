# NGO 案件管理系統

> 一個完整的 NGO 案件管理、活動管理、物資管理與志工協作平台

## 📋 專案簡介

這是一個為非營利組織（NGO）設計的全功能管理系統，提供完整的案件追蹤、活動管理、物資配送、志工協作等功能。系統採用現代化的前後端分離架構，並提供雙前端設計（管理後台 + 用戶前台），滿足不同角色的使用需求。

### 🎯 專案目標

- **提升工作效率**：數位化案件管理流程，減少人工作業
- **資料視覺化**：即時統計圖表，輔助決策分析
- **權限管理**：完整的角色權限系統（管理員、督導、員工）
- **跨平台**：響應式設計，支援桌面與行動裝置

---

## 🛠 技術棧

### 後端 (Backend API)
- **框架**：ASP.NET Core 9.0 (Web API)
- **資料庫**：SQL Server (Entity Framework Core)
- **架構**：RESTful API
- **ORM**：Entity Framework Core
- **認證**：JWT Token

### 前端 - 管理後台 (Admin Dashboard)
- **框架**：React 18
- **建置工具**：Vite
- **UI 框架**：Ant Design / Material-UI
- **圖表**：Chart.js / Recharts
- **狀態管理**：React Context API
- **HTTP 客戶端**：Axios

### 前端 - 用戶前台 (User Portal)
- **框架**：ASP.NET Core MVC (.NET 8)
- **視圖引擎**：Razor Pages
- **認證**：Cookie Authentication
- **第三方登入**：Google OAuth
- **金流**：ECPay

---

## 🏗 系統架構

```
┌─────────────────────────────────────────────┐
│         SQL Server (NGOPlatformDb)          │
│         - 20 張資料表                        │
│         - 2 個 Views                        │
│         - 1 個 Trigger                      │
└──────────────────┬──────────────────────────┘
                   │
                   │ Entity Framework Core
                   │
        ┌──────────┴──────────┐
        │                     │
        ▼                     ▼
┌───────────────────┐ ┌───────────────────┐
│   後端 API        │ │   後端 API        │
│   (Port 5264)     │ │   (Port 5264)     │
│   ASP.NET Core 9  │ │   ASP.NET Core 9  │
└─────────┬─────────┘ └─────────┬─────────┘
          │                     │
          │ REST API            │ REST API / Direct DB
          │                     │
          ▼                     ▼
┌───────────────────┐ ┌───────────────────┐
│  React 前端       │ │   MVC 前端        │
│  (Port 5173)      │ │   (Port 5066)     │
│  管理後台         │ │   用戶前台        │
│  - 案件管理       │ │   - 活動報名      │
│  - 統計分析       │ │   - 物資捐贈      │
│  - 權限管理       │ │   - 緊急需求      │
└───────────────────┘ └───────────────────┘
```

---

## ✨ 核心功能

### 📊 管理後台 (React - Port 5173)

#### 1. Dashboard 儀表板
- 📈 即時統計數據（個案、志工、活動）
- 📊 性別分佈圖表
- 🗺 城市分佈地圖
- 📉 困難類型分析
- 📅 近期活動概覽

#### 2. 案件管理 (Case Management)
- 📝 新增/編輯/刪除個案
- 🔍 進階搜尋與篩選
- 📋 個案狀態追蹤
- 📄 個案歷史記錄
- 📊 個案統計報表

#### 3. 活動管理 (Activity Management)
- 🎯 活動建立與編輯
- 👥 參與人數管理
- 📅 活動行程安排
- 📈 活動成效追蹤
- 📸 活動圖片上傳

#### 4. 物資管理 (Supply Management)
- 📦 物資庫存管理
- 🏷 物資分類系統
- 💰 物資價值統計
- ⚠️ 低庫存警示
- 📊 物資發放記錄

#### 5. 帳號管理 (Account Management)
- 👤 員工帳號管理
- 🔐 角色權限設定
- 📧 帳號啟用/停用
- 🔄 密碼重置

#### 6. 行程管理 (Schedule Management)
- 📅 個人行程管理
- 🔔 行程提醒通知
- 📋 團隊行程檢視

### 🌐 用戶前台 (MVC - Port 5066)

#### 1. 活動報名
- 📋 瀏覽開放活動
- ✅ 線上報名功能
- 📧 報名確認通知

#### 2. 物資捐贈
- 🛒 物資選購
- 💳 線上捐款 (ECPay)
- 📦 物資配送追蹤

#### 3. 緊急需求
- 🚨 緊急物資需求
- 💰 緊急募資進度
- 🎯 優先級標示

#### 4. 用戶認證
- 🔐 一般登入/註冊
- 🌐 Google OAuth 登入
- 🔑 密碼重置功能

---

## 🗄 資料庫架構

### 核心資料表
- **Cases**：個案資料
- **Workers**：員工/志工資料
- **Activities**：活動資料
- **Supplies**：物資資料
- **SupplyCategories**：物資分類
- **Schedules**：行程資料
- **RegularSuppliesNeeds**：定期物資需求
- **EmergencySupplyNeeds**：緊急物資需求
- **RegularDistributionBatch**：物資發放批次
- **PasswordResetTokens**：密碼重置 Token

### 關聯設計
- 一對多：Workers → Cases
- 一對多：Workers → Activities
- 多對多：Activities ↔ Cases (透過中介表)
- 一對多：SupplyCategories → Supplies

---

## 🚀 快速啟動

### 📋 環境需求

- **Node.js**: 18.x 或更高版本
- **.NET SDK**: 8.0 或 9.0
- **SQL Server**: 2019 或更高版本（或 SQL Server Express）
- **Git**: 最新版本

### 📥 安裝步驟

#### 1. Clone 專案

```bash
git clone https://github.com/your-username/NGO-Admin-System.git
cd NGO-Admin-System
```

#### 2. 安裝後端 API 依賴

```bash
cd NGO-Admin-System-WebAPI
dotnet restore
dotnet build
```

#### 3. 安裝 React 前端依賴

```bash
cd ../NGO-Admin-System
npm install
```

#### 4. 安裝 MVC 前端依賴（可選）

```bash
cd ../NGO-User-Case/NGOPlatformWeb
dotnet restore
dotnet build
```

#### 5. 設定資料庫

修改 `NGO-Admin-System-WebAPI/appsettings.json` 中的連線字串：

```json
{
  "ConnectionStrings": {
    "NGODb": "Server=YOUR_SERVER;Database=NGOPlatformDb;Trusted_Connection=True;Encrypt=False"
  }
}
```

執行資料庫遷移（如果有）：

```bash
cd NGO-Admin-System-WebAPI
dotnet ef database update
```

#### 6. 設定 React 環境變數

修改 `NGO-Admin-System/.env.development`：

```env
VITE_API_BASE_URL=http://localhost:5264/api
```

---

## 🎮 啟動系統

### 方法 1：手動啟動（開發模式）

**終端機 1 - 啟動後端 API**
```bash
cd NGO-Admin-System-WebAPI
dotnet run
```
> 後端 API 運行於：http://localhost:5264

**終端機 2 - 啟動 React 前端**
```bash
cd NGO-Admin-System
npm run dev
```
> React 前端運行於：http://localhost:5173

**終端機 3 - 啟動 MVC 前端（可選）**
```bash
cd NGO-User-Case/NGOPlatformWeb
dotnet run
```
> MVC 前端運行於：http://localhost:5066

### 方法 2：使用啟動腳本（Windows）

```bash
# 使用 Cloudflare Tunnel（推薦用於展示）
.\scripts\啟動展示系統-Cloudflare.bat
```

---

## 🔐 測試帳號

### 管理後台 (React)

| 角色 | Email | 密碼 | 權限 |
|------|-------|------|------|
| 管理員 | admin@ngo.org | Admin123! | 完整權限 |
| 督導 | supervisor@ngo.org | Super123! | 審核與管理 |
| 員工 | staff@ngo.org | Staff123! | 基本操作 |

### 用戶前台 (MVC)

請使用前台註冊功能或 Google 登入

---

## 📊 專案統計（範例資料）

- **個案總數**：32（今年新增 32）
- **志工總數**：24
- **活動總數**：31
- **物資種類**：50+
- **涵蓋城市**：18 個縣市

---

## 📁 專案結構

```
NGO-Admin-System/
├── NGO-Admin-System/              # React 前端（管理後台）
│   ├── src/
│   │   ├── components/            # React 元件
│   │   ├── pages/                 # 頁面元件
│   │   ├── services/              # API 服務
│   │   ├── contexts/              # 狀態管理
│   │   └── utils/                 # 工具函數
│   ├── public/                    # 靜態資源
│   ├── .env.development           # 開發環境變數
│   └── package.json
│
├── NGO-Admin-System-WebAPI/       # .NET 9 後端 API
│   ├── Controllers/
│   │   ├── AccountManagement/     # 帳號管理
│   │   ├── ActivityManagement/    # 活動管理
│   │   ├── CaseManagement/        # 案件管理
│   │   ├── Dashboard/             # 統計儀表板
│   │   ├── ScheduleManagement/    # 行程管理
│   │   └── SupplyManagement/      # 物資管理
│   ├── Models/
│   │   ├── Entities/              # 資料表實體
│   │   └── Infrastructure/        # DbContext
│   ├── appsettings.json           # 配置檔案
│   └── Program.cs
│
├── NGO-User-Case/NGOPlatformWeb/  # MVC 前端（用戶前台）
│   ├── Controllers/               # MVC 控制器
│   ├── Views/                     # Razor 視圖
│   ├── Models/                    # 資料模型
│   ├── Services/                  # 業務邏輯
│   └── appsettings.json
│
├── scripts/                       # 啟動腳本
│   ├── 啟動展示系統-Cloudflare.bat
│   └── README.md
│
├── docs/                          # 文檔
├── .gitignore
├── README.md
└── QUICK-START.md
```

---

## 🔧 開發指南

### API 端點範例

```
GET    /api/Dashboard/stats              - 獲取統計數據
GET    /api/Dashboard/gender-distribution - 性別分佈
GET    /api/Case                          - 獲取所有個案
POST   /api/Case                          - 新增個案
PUT    /api/Case/{id}                     - 更新個案
DELETE /api/Case/{id}                     - 刪除個案
GET    /api/Activity                      - 獲取所有活動
POST   /api/Auth/login                    - 登入
GET    /api/Supply                        - 獲取物資列表
```

### 開發環境建議

- **IDE**：Visual Studio 2022 / VS Code
- **資料庫工具**：SQL Server Management Studio (SSMS)
- **API 測試**：Postman / Thunder Client
- **版本控制**：Git

---

## 🌐 部署

### 後端 API 部署
- 可部署至 Azure App Service
- 或使用 Docker 容器化

### 前端部署
- React：可部署至 Vercel、Netlify、Azure Static Web Apps
- MVC：可部署至 Azure App Service、IIS

### 資料庫
- 使用 Azure SQL Database
- 或自架 SQL Server

---

## 🤝 貢獻指南

歡迎提交 Issue 或 Pull Request！

1. Fork 此專案
2. 創建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

---

## 📄 授權

此專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 檔案

---

## 👨‍💻 開發者

- **姓名**：[你的名字]
- **Email**：[你的 Email]
- **GitHub**：[你的 GitHub]
- **LinkedIn**：[你的 LinkedIn]

---

## 📞 聯絡方式

如有任何問題或建議，歡迎透過以下方式聯繫：

- 📧 Email: [你的 Email]
- 💬 GitHub Issues: [專案 Issues 頁面]
- 🌐 Website: [個人網站]

---

## 🙏 致謝

感謝所有參與此專案開發的夥伴，以及提供技術支援的社群。

---

## 📸 系統截圖

### Dashboard 儀表板
![Dashboard](docs/screenshots/dashboard.png)

### 案件管理
![Case Management](docs/screenshots/case-management.png)

### 活動管理
![Activity Management](docs/screenshots/activity-management.png)

---

**⭐ 如果這個專案對你有幫助，請給個 Star！**
