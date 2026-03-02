# 面試檢討 - 2026/02/10

> 自我評分：40/100
> 面試官特徵：技術底子深厚，會追問細節

---

## Q1: 專案結構 & WebAPI 是誰在使用？

### 我的回答
- Repo 包含所有平台
- WebAPI Backend 主要給 React 端的社工管理平台使用
- MVC 資料夾是傳統 MVC 架構，給民眾/個案端使用

### 評估：OK
回答方向正確，清楚說明了各平台的角色分工。

### 補充學習
- 可以進一步強調 **前後端分離（SPA + API）** vs **傳統 Server-Side Rendering（MVC）** 的架構選擇差異
- 提到為什麼社工端選 React SPA（互動性高、即時更新）、民眾端選 MVC（SEO 友善、表單為主）會更有說服力

---

## Q2: Code First vs DB First？

### 我的回答
- 不知道這個概念，只提到有用 EF 做資料庫操作
- 猜了 Code First，但實際上專案是 **DB First**
- 面試官友善糾正

### 評估：不及格

### 正確觀念

| 方式 | 說明 | 適用場景 |
|------|------|----------|
| **Code First** | 先寫 C# Entity Class → EF Migration 自動產生資料庫 | 新專案、開發主導 |
| **DB First** | 先設計好資料庫 → EF Scaffold 反向產生 Entity Class | 既有資料庫、DBA 主導 |
| **Model First** | 用視覺化設計工具產生 Model → 同時產生 DB（已過時） | 幾乎不用了 |

**怎麼判斷自己的專案是哪種？**
- 有 `Migrations/` 資料夾 → Code First
- 有用 `Scaffold-DbContext` 指令 → DB First
- 先在 SSMS 建表再用 EF 對應 → DB First

### 關鍵指令
```bash
# DB First: 從既有資料庫產生 Entity
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer

# Code First: 從 Entity 產生 Migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Q3: appsettings 怎麼區分環境？

### 我的回答
- 不清楚機制，只提到 interface 的概念
- 自評大錯特錯

### 評估：不及格

### 正確觀念

ASP.NET Core 透過 **ASPNETCORE_ENVIRONMENT** 環境變數自動載入對應的設定檔：

```
appsettings.json                  ← 基底設定（所有環境共用）
appsettings.Development.json      ← 開發環境覆蓋
appsettings.Staging.json          ← 測試環境覆蓋
appsettings.Production.json       ← 正式環境覆蓋
```

**載入順序（後面覆蓋前面）：**
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. 環境變數
4. 命令列參數

**設定方式：**
```bash
# launchSettings.json 裡設定（開發時）
"ASPNETCORE_ENVIRONMENT": "Development"

# 正式部署時設定環境變數
set ASPNETCORE_ENVIRONMENT=Production
```

**程式碼層面：**
```csharp
// Program.cs - 框架自動處理，不需手動寫
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration 已自動合併所有設定檔
```

### 重點
- 這不是靠 interface，是 **框架內建的設定檔覆蓋機制**
- `appsettings.json` 放共用設定，環境專屬檔放敏感或環境特定值
- Production 環境不應把密鑰放設定檔，應用環境變數或 Azure Key Vault

---

## Q4: DI 生命週期 - Scoped / Singleton / Transient

### 我的回答
- 有準備，回答 OK

### 評估：OK

### 快速複習

| 生命週期 | 說明 | 範例 |
|----------|------|------|
| **Transient** | 每次注入都建立新 instance | 輕量、無狀態的工具類 |
| **Scoped** | 每個 HTTP Request 共用一個 instance | DbContext、Service |
| **Singleton** | 整個應用程式只有一個 instance | 設定檔、快取、HttpClient Factory |

---

## Q5: 多對多關聯 & 中介表設計

### 我的回答
- 正確說出需要中介表
- 但 FK/PK 設定答錯：說「種類 FK、商品 PK」

### 評估：部分正確

### 正確設計

```
商品表 Products          種類表 Categories
┌─────────────┐         ┌─────────────┐
│ ProductId PK│         │ CategoryId PK│
│ Name        │         │ Name         │
└─────────────┘         └──────────────┘

        中介表 ProductCategories
        ┌──────────────────────────┐
        │ ProductId  FK → Products │  ← 兩個都是 FK
        │ CategoryId FK → Categories│  ← 兩個都是 FK
        │                          │
        │ 複合 PK = (ProductId,    │  ← 兩個 FK 一起組成複合 PK
        │           CategoryId)    │
        └──────────────────────────┘
```

### 關鍵觀念
- 中介表的兩個欄位 **都是 FK**（分別參照兩張主表）
- 這兩個 FK **一起組成複合主鍵 (Composite PK)**，確保同一組合不會重複
- EF Core 寫法：
```csharp
// Entity
public class ProductCategory
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

// DbContext OnModelCreating
modelBuilder.Entity<ProductCategory>()
    .HasKey(pc => new { pc.ProductId, pc.CategoryId }); // 複合 PK
```

---

## Q6: Cookie & Session

### 我的回答
- 有準備，回答沒問題

### 評估：OK

### 快速複習
- **Cookie**：存在客戶端瀏覽器，每次請求自動帶上，有大小限制（4KB）
- **Session**：存在伺服器端，用 Session ID（透過 Cookie 傳遞）識別用戶
- Session 預設存在記憶體，可改用 Redis/SQL Server 做分散式 Session

---

## Q7: 資料庫設計流程 & AI 協作

### 我的回答
- 用 Gemini 先產生資料庫設計規範
- 用 Mermaid 做基礎 ER 圖
- 用 Claude Code 開發

### 評估：OK（但可以更好）

### 可以改進的回答方式
1. **需求分析** → 先釐清業務需求和實體關係
2. **概念設計** → 用 ER Diagram 畫出實體和關聯
3. **邏輯設計** → 正規化（1NF → 2NF → 3NF）避免資料冗餘
4. **物理設計** → 決定資料型態、索引、約束條件
5. **AI 輔助** → 用 AI 加速產出初版，但自己要能 review 和調整

> 面試官想聽的不只是「用什麼工具」，而是你 **理不理解資料庫設計的方法論**

---

## 金流（ECPay）- 未被問到

面試官沒有追問金流相關內容。

---

## 總結 & 下次準備重點

### 答得好的（繼續保持）
- [x] 專案架構介紹
- [x] DI 生命週期
- [x] Cookie & Session

### 需要加強的（優先補強）
- [ ] **EF Core: Code First vs DB First** — 要能判斷專案用哪種、各自的指令和流程
- [ ] **appsettings 環境設定機制** — ASP.NET Core 的設定檔載入順序和覆蓋機制
- [ ] **資料庫多對多設計** — 中介表的 FK/PK 設定要能精確說明
- [ ] **資料庫設計方法論** — 不只講工具，要能講出正規化、ER Model 等概念

### 心態調整
- 不知道的誠實說不知道，比亂猜好（面試官看得出來）
- 答錯被糾正時態度好是加分的，但要減少需要被糾正的次數
- 實作經驗很重要，但 **底層觀念** 同樣要扎實
