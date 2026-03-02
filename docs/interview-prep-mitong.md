# 面試準備：彌通數位有限公司

> 專案：NGO Management System（非營利組織案件管理系統）
> 準備日期：2026-02-11

---

## 一、30 秒自我介紹（專案部分）

> 我獨立開發了一套 NGO 案件管理系統，採前後端分離架構：後端用 **ASP.NET Core 9 WebAPI**，前端有兩套 — **React 18 員工管理後台**和 **ASP.NET Core MVC 用戶前台**，資料庫用 SQL Server。
>
> 系統整合了 **OpenAI 三項 AI 功能**（GPT 文案優化、DALL-E 圖片生成、Whisper 語音轉文字）、**ECPay 綠界金流支付**、**Google OAuth 第三方登入**，並透過 Cloudflare Tunnel 部署到固定公開網域供展示。

---

## 二、系統架構圖

```
                      ┌──────────────────┐
                      │  Cloudflare      │
                      │  Tunnel          │
                      │  (固定網域部署)    │
                      └────────┬─────────┘
               ┌───────────────┼───────────────┐
               ▼               ▼               ▼
      React Admin         MVC Portal       API Server
      (React 18+Vite)     (.NET 8)         (.NET 9)
      員工管理後台          用戶前台          統一 API
      :5173               :5066             :5264
               └───────────────┼───────────────┘
                               ▼
                      ┌──────────────────┐
                      │   SQL Server     │
                      │   EF Core        │
                      └──────────────────┘
```

**架構設計理由：**

- 前後端完全分離，Backend 是純 API，不綁定任何前端
- React 做內部管理（SPA 體驗好、互動多），MVC 做對外用戶端（SEO 友善、開發快）
- 兩套前端共用同一個 API，避免重複邏輯
- 三層架構：Repository Pattern → Service Layer → Controller

---

## 三、技術棧對照（對應彌通需求）

| 彌通要求 | 我的專案對應 | 補充說明 |
|---------|-------------|---------|
| **C#** | ASP.NET Core 9 WebAPI + MVC .NET 8 | 雙框架實戰，熟悉 DI、Middleware、EF Core |
| **JavaScript** | React 18 + TypeScript + Vite | 完整 SPA 開發，含狀態管理、路由、API 串接 |
| **MySQL** | SQL Server + EF Core (Database-First) | 關聯式資料庫概念相通，熟悉 LINQ、分頁查詢、Transaction |
| **Redis** | JWT Token 管理 + Session 概念 | 理解快取應用場景，可快速上手 Redis |
| **AWS** | Cloudflare Tunnel 部署 | 有雲端部署思路，理解反向代理、DNS 設定 |
| **Docker** | Cloudflare Tunnel + 多服務管理 | 有多服務部署經驗，Docker 持續學習中 |
| **VueJS** | React 18 經驗 | 元件化思維相通，Vue 可快速轉換 |

---

## 四、核心功能模組

### 4.1 員工後台（React）

| 功能 | 技術亮點 |
|------|---------|
| JWT 身份驗證 | Role-based Claims（管理員/督導/員工三級權限） |
| Dashboard | Recharts 圖表視覺化，統計數據即時呈現 |
| 個案管理 | CRUD、多條件搜尋、圖片上傳、分頁 |
| 活動管理 | CRUD、報名審核、AI 文案優化、AI 圖片生成 |
| 物資管理 | 庫存管理、低庫存警示 |
| 語音轉文字 | OpenAI Whisper 整合，錄音即轉文字填入表單 |

### 4.2 用戶前台（MVC）

| 功能 | 技術亮點 |
|------|---------|
| Google OAuth 登入 | 自動建立用戶、頭像同步 |
| 活動報名 | 即時名額檢查（DB Trigger 控制上限） |
| 物資購物 | 購物車、下單流程 |
| ECPay 金流支付 | CheckMacValue 雙重驗證、回調處理 |
| 成就系統 | Gamification，多表 LINQ 查詢即時計算 |

---

## 五、三個技術故事（面試用）

### 故事 A：AI 整合 — Factory Pattern 的實際應用

**情境：** 系統需要同時支援 OpenAI Direct API 和 Azure OpenAI 兩個供應商。

**做法：**
- 設計 `OpenAIClientFactory`（Singleton），用 Factory Pattern 統一建立 AI Client
- 透過 `appsettings.json` 的 `AI:Provider` 設定切換供應商
- 一個 API Key 就能啟用三項 AI 功能：
  - **GPT-4o-mini**：活動文案優化（情感化行銷文案）
  - **DALL-E 3**：活動封面圖片生成
  - **Whisper**：語音轉文字（個案記錄用）

**結果：** 切換 AI 供應商完全不需要改程式碼，只改設定檔。

**展示能力：** 設計模式、依賴注入、抽象化思維、第三方 API 整合

---

### 故事 B：ECPay 金流 — 安全性與資料一致性

**情境：** 整合 ECPay 綠界支付，需要處理安全驗證和庫存扣減的併發問題。

**做法：**
- 實作 **CheckMacValue 雙重驗證**：
  - 送出付款時產生 SHA256 簽章
  - 收到回調時驗證簽章，防止偽造請求和重放攻擊
- 庫存扣減用 **Raw SQL Transaction**，避免 Race Condition
- 建立完整的交易紀錄狀態管理（Pending → Paid → Failed）

**結果：** 支付流程安全可靠，庫存不會超賣。

**展示能力：** 資安意識、交易一致性、第三方金流 API 整合

---

### 故事 C：密碼系統升級 — 零停機漸進式重構

**情境：** 原本系統密碼是明文儲存，需要升級但不能讓現有用戶無法登入。

**做法：**
- 升級到 **Argon2id**（OWASP 推薦的密碼 Hash 演算法）
- 設計向下相容機制：登入時檢測密碼格式，舊格式自動遷移到 Argon2id
- 使用固定時間比較（Constant-time comparison）防止 Timing Attack

**結果：** 零停機完成安全升級，用戶無感知切換。

**展示能力：** 資安意識、重構策略、向下相容設計

---

## 六、設計模式與架構決策

| 模式 | 應用場景 | 說明 |
|------|---------|------|
| **Repository Pattern** | 資料存取層 | `ICaseRepository` 分離 DB 操作與商業邏輯 |
| **Factory Pattern** | AI Client 建立 | `OpenAIClientFactory` 依設定建立不同 Provider 的 Client |
| **Strategy Pattern** | 檔案儲存 | `IFileStorageService` 介面，Local / Azure Blob 兩種實作 |
| **三層架構** | 全系統 | Controller → Service → Repository，職責分離 |
| **統一回應格式** | API 設計 | `ApiResponse<T>` 包含 Success、Message、Data、分頁資訊 |

---

## 七、常見面試問題預備

### Q：為什麼前端做兩套？

> 員工後台需要大量互動操作（表格編輯、即時搜尋、拖拉），React SPA 體驗最好。用戶前台重視 SEO、頁面載入速度，MVC Server-Side Rendering 更適合。兩套各取所長，共用同一個 API 避免重複邏輯。

### Q：你的資料庫設計怎麼做的？

> 採用 Database-First + EF Core Scaffold。有用 Trigger 管理活動報名人數上限（自動檢查是否已滿），有建立 View 優化前端常用查詢。資料完整性靠 Unique Constraint（身分證字號）和 Foreign Key 保護。

### Q：怎麼處理認證授權？

> 兩套機制：後台用 JWT + Role-based Claims，Token 存 localStorage，三種角色（管理員/督導/員工）對應不同權限。前台用 Google OAuth + Cookie Authentication，首次登入自動建立用戶帳號。

### Q：如果要導入 Docker，你會怎麼做？

> 我會為三個服務各寫一個 Dockerfile，再用 docker-compose 統一管理。SQL Server 也可以容器化。目前的 Cloudflare Tunnel 部署經驗讓我理解多服務編排的概念，轉換到 Docker 會很自然。

### Q：團隊協作經驗？

> 雖然這是個人專案，但我用 Git 做版本管理、寫了 5600+ 行技術文檔、模組化設計（Controller / Service / Repository 分層），讓程式碼結構清晰、方便多人協作。API 統一回應格式也是考量前後端協作的設計。

### Q：離職原因？（通用版）

> 希望在技術上有更多成長空間，接觸更完整的開發流程和團隊協作。這段期間我利用時間獨立完成了這個全端專案，證明了我的學習能力和技術廣度。

---

## 八、加分亮點

- 完整技術文檔 **5600+ 行**，包含架構說明、部署指南、測試帳號
- **FluentValidation** 伺服器端驗證，包含台灣身分證字號檢查碼演算法
- **Cloudflare Tunnel** 固定網域部署，隨時可線上展示
- **成就系統** Gamification 設計，複雜多表 LINQ 查詢即時計算
- 前端使用 **MUI 7 + Recharts**，UI 完成度高
- **TypeScript** 型別安全，所有 DTO 都有 Interface 定義

---

## 九、展示連結

| 服務 | 網址 |
|------|------|
| 用戶前台 | https://ngo-management-hub.com |
| 管理後台 | https://admin.ngo-management-hub.com |
| API | https://api.ngo-management-hub.com |

> 注意：需先啟動本機服務和 Cloudflare Tunnel 才能存取

### 測試帳號

| 系統 | 帳號 | 密碼 |
|------|------|------|
| React 管理後台 | admin@ngo.org | Admin123! |
| MVC 用戶前台 | test.user@example.com | Test123! |

---

## 十、面試心態提醒

- 彌通面試風格**輕鬆、重對話**，不用太緊張
- 用**說故事**的方式帶技術細節，不要只列技術名詞
- 重點放在「**為什麼這樣設計**」和「**遇到什麼問題、怎麼解決**」
- 遇到不會的技術（如 Vue、Docker）誠實說，但強調**學習能力**和**可轉移經驗**
- 可以主動提出線上 Demo，展示實際系統運作
