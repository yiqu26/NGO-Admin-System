# NGO Management System - 完整專案說明文件

> **版本**: 1.0.0
> **最後更新**: 2025-01-29
> **專案類型**: 非營利組織管理平台
> ㄔ└── CaseDto.cs                      # 個案 DTO
│
├── Services/
│   ├── CaseService.cs
│   ├── ICaseService.cs
│   ├── AzureOpenAIService.cs
│   ├── PasswordService.cs
│   ├── IPasswordService.cs
│   └── JwtHelper.cs
│
├── Repositories/
│   ├── CaseRepository.cs
│   └── ICaseRepository.cs
│
├── Validators/
│   ├── CreateCaseDtoValidator.cs
│   └── UpdateCaseDtoValidator.cs
│
├── Program.cs                          # 應用程式入口
├── appsettings.json                    # 配置檔
├── appsettings.Production.json
├── Dockerfile
└── NGO_WebAPI_Backend.csproj
```

### 4.3 前端目錄結構

```
react-admin/
├── src/
│   ├── pages/                          # 頁面組件
│   │   ├── Dashboard.tsx
│   │   ├── CaseManagement.tsx
│   │   ├── ActivityManagement.tsx
│   │   ├── SuppliesManagement.tsx
│   │   ├── AccountManagement.tsx
│   │   ├── Login.tsx
│   │   └── schedule/
│   │       └── CalendarManagement.tsx
│   │
│   ├── components/                     # 可重用組件
│   │   ├── CaseManagementPage/
│   │   │   ├── AddCaseTab.tsx
│   │   │   └── SearchEditCaseTab.tsx
│   │   │
│   │   ├── ActivityManagementPage/
│   │   │   ├── ActivityManagement.tsx
│   │   │   ├── NewActivityForm.tsx
│   │   │   ├── RegistrationReviewMain.tsx
│   │   │   ├── CaseRegistrationReview.tsx
│   │   │   └── PublicRegistrationReview.tsx
│   │   │
│   │   ├── SuppliesManagementPage/
│   │   │   ├── InventoryTab.tsx
│   │   │   ├── EmergencyRequestTab.tsx
│   │   │   ├── RegularRequestTab.tsx
│   │   │   └── DistributionTab.tsx
│   │   │
│   │   ├── AccountManagement/
│   │   │   ├── AddAccountDialog.tsx
│   │   │   └── EditAccountDialog.tsx
│   │   │
│   │   ├── layout/
│   │   │   ├── MainLayout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   │
│   │   ├── shared/                     # 共用組件
│   │   │   ├── PageHeader.tsx
│   │   │   ├── PageContainer.tsx
│   │   │   ├── StatCard.tsx
│   │   │   ├── LoadingSpinner.tsx
│   │   │   ├── ErrorDialog.tsx
│   │   │   ├── ConfirmDialog.tsx
│   │   │   ├── GenderChart.tsx
│   │   │   ├── RegionChart.tsx
│   │   │   ├── DifficultyRadarChart.tsx
│   │   │   ├── AIOptimizeButton.tsx
│   │   │   ├── SpeechToText.tsx
│   │   │   ├── GoogleMapSelector.tsx
│   │   │   └── ...
│   │   │
│   │   └── AuthProvider.tsx
│   │
│   ├── services/                       # API 服務
│   │   ├── shared/
│   │   │   ├── api.ts                  # Axios 配置
│   │   │   └── newsService.ts
│   │   │
│   │   ├── accountManagement/
│   │   │   ├── authService.ts
│   │   │   ├── azureService.ts
│   │   │   └── accountService.ts
│   │   │
│   │   ├── caseManagement/
│   │   │   ├── caseService.ts
│   │   │   ├── caseNewService.ts
│   │   │   └── caseSpeechService.ts
│   │   │
│   │   ├── activityManagement/
│   │   │   ├── activityService.ts
│   │   │   ├── activityAIService.ts
│   │   │   ├── activityImageService.ts
│   │   │   └── registrationService.ts
│   │   │
│   │   ├── supplyManagement/
│   │   │   ├── supplyService.ts
│   │   │   ├── emergencySupplyNeedService.ts
│   │   │   └── distributionBatchService.ts
│   │   │
│   │   ├── dashboard/
│   │   │   └── dashboardService.ts
│   │   │
│   │   └── schedule/
│   │       ├── scheduleService.ts
│   │       └── calendarService.ts
│   │
│   ├── hooks/
│   │   ├── useAuth.tsx
│   │   ├── useUserRole.tsx
│   │   └── useNotificationStatus.tsx
│   │
│   ├── contexts/
│   │   └── NotificationContext.tsx
│   │
│   ├── routes/
│   │   └── index.tsx
│   │
│   ├── config/
│   │   ├── env.ts
│   │   ├── environment.ts
│   │   └── azureConfig.ts
│   │
│   ├── types/
│   │   ├── userTypes.ts
│   │   └── caseAI.ts
│   │
│   ├── styles/
│   │   ├── theme.ts
│   │   ├── commonStyles.ts
│   │   └── global.css
│   │
│   ├── utils/
│   │   ├── dateHelper.ts
│   │   ├── validation.ts
│   │   ├── speechParser.ts
│   │   ├── lazyLoad.ts
│   │   └── performanceMonitor.ts
│   │
│   ├── App.tsx
│   └── main.tsx
│
├── public/
├── .env.development
├── .env.production
├── package.json
├── vite.config.ts
└── tsconfig.json
```

---

## 5. 後端 API

### 5.1 API 端點總覽

#### 個案管理 (Case Management)

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/case` | 取得個案列表 (分頁) |
| GET | `/api/case/{id}` | 取得個案詳情 |
| POST | `/api/case` | 建立個案 |
| PUT | `/api/case/{id}` | 更新個案 |
| DELETE | `/api/case/{id}` | 刪除個案 |
| GET | `/api/case/search` | 搜尋個案 |
| POST | `/api/case/upload/profile-image` | 上傳頭像 |
| POST | `/api/case/speech-to-text` | 語音轉文字 |

#### 活動管理 (Activity Management)

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/activity` | 取得活動列表 |
| GET | `/api/activity/{id}` | 取得活動詳情 |
| POST | `/api/activity` | 建立活動 |
| PUT | `/api/activity/{id}` | 更新活動 |
| DELETE | `/api/activity/{id}` | 刪除活動 |
| POST | `/api/activity/ai-optimize` | AI 優化活動描述 |
| POST | `/api/activity/generate-image` | AI 生成活動圖片 |
| GET | `/api/registration-review` | 報名審核列表 |
| PUT | `/api/registration-review/{id}` | 審核報名 |

#### 物資管理 (Supply Management)

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/supply` | 物資列表 |
| POST | `/api/supply` | 建立物資 |
| PUT | `/api/supply/{id}` | 更新物資 |
| DELETE | `/api/supply/{id}` | 刪除物資 |
| GET | `/api/emergencysupplyneed` | 緊急需求列表 |
| POST | `/api/emergencysupplyneed` | 建立緊急需求 |
| GET | `/api/regularsuppliesneed` | 定期需求列表 |
| POST | `/api/regulardistributionbatch` | 建立配送批次 |

#### 帳號管理 (Account Management)

| 方法 | 端點 | 說明 |
|------|------|------|
| POST | `/api/auth/login` | 登入 |
| POST | `/api/auth/logout` | 登出 |
| GET | `/api/account` | 帳號列表 |
| POST | `/api/account` | 建立帳號 |
| PUT | `/api/account/{id}` | 更新帳號 |
| DELETE | `/api/account/{id}` | 刪除帳號 |

#### 儀表板 (Dashboard)

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/dashboard/statistics` | 統計數據 |
| GET | `/api/dashboard/case-distribution` | 個案分布 |
| GET | `/api/dashboard/gender-distribution` | 性別分布 |

### 5.2 統一回應格式

#### 標準回應 (ApiResponse)

```json
{
  "success": true,
  "message": "操作成功",
  "data": { ... },
  "error": null,
  "timestamp": "2025-01-29T10:00:00Z"
}
```

#### 分頁回應 (PagedApiResponse)

```json
{
  "success": true,
  "message": "查詢成功",
  "data": [...],
  "pageInfo": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 50,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "timestamp": "2025-01-29T10:00:00Z"
}
```

### 5.3 核心資料模型

#### Case (個案)

```csharp
public class Case
{
    public int CaseId { get; set; }              // 主鍵
    public string? Name { get; set; }            // 姓名
    public string? Phone { get; set; }           // 電話
    public string? IdentityNumber { get; set; }  // 身分證 (唯一)
    public DateOnly? Birthday { get; set; }      // 出生日期
    public int? WorkerId { get; set; }           // 負責員工
    public string? Description { get; set; }     // 個案描述
    public DateTime? CreatedAt { get; set; }     // 建立時間
    public string? Status { get; set; }          // 狀態
    public string? Email { get; set; }           // 信箱
    public string? Gender { get; set; }          // 性別
    public string? ProfileImage { get; set; }    // 頭像 URL
    public string? City { get; set; }            // 城市
    public string? District { get; set; }        // 鄉鎮區
    public string? DetailAddress { get; set; }   // 詳細地址

    // 關聯
    public virtual Worker? Worker { get; set; }
    public virtual ICollection<CaseActivityRegistration> CaseActivityRegistrations { get; set; }
    public virtual ICollection<EmergencySupplyNeed> EmergencySupplyNeeds { get; set; }
}
```

#### Worker (員工)

```csharp
public class Worker
{
    public int WorkerId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }        // Argon2 加密
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }            // Admin/Supervisor/Staff
    public DateTime? CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

#### Activity (活動)

```csharp
public class Activity
{
    public int ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public int? MaxParticipants { get; set; }
    public int? CurrentParticipants { get; set; }
    public string? Status { get; set; }          // Open/InProgress/Closed
    public string? ImageUrl { get; set; }
    public int? WorkerId { get; set; }
}
```

---

## 6. 前端 React 管理後台

### 6.1 路由配置

```typescript
// 受保護路由 (需登入)
/                           → Dashboard (儀表板)
/dashboard                  → Dashboard
/case-management           → CaseManagement (個案管理)
/activity-management       → ActivityManagement (活動管理)
/supplies-management       → SuppliesManagement (物資管理)
/calendar-management       → CalendarManagement (行程管理)
/account-management        → AccountManagement (帳號管理)

// 公開路由
/login                     → Login (登入頁)
```

### 6.2 狀態管理

#### 認證狀態 (useAuth)

```typescript
interface AuthState {
  isAuthenticated: boolean;
  user: UnifiedUser | null;
  loginMethod: 'database' | 'azure_ad';
  loading: boolean;
  error: string | null;
}

// 主要方法
loginWithDatabase(email: string, password: string): Promise<void>
loginWithAzure(): Promise<void>
logout(): void
getAccessToken(): string | null
```

#### 角色權限 (useUserRole)

```typescript
interface UserRoleHook {
  isAdmin: boolean;      // 管理員
  isSupervisor: boolean; // 督導
  isStaff: boolean;      // 員工
  role: string;
}
```

### 6.3 API 服務層

```typescript
// api.ts - Axios 配置
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
});

// 請求攔截器 - 自動添加 Token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 回應攔截器 - 401 自動登出
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

### 6.4 核心組件

| 組件 | 位置 | 用途 |
|------|------|------|
| `MainLayout` | layout/MainLayout.tsx | 主版型 (含側邊欄) |
| `ProtectedRoute` | layout/ProtectedRoute.tsx | 路由保護 |
| `Sidebar` | layout/Sidebar.tsx | 側邊導航 |
| `PageHeader` | shared/PageHeader.tsx | 頁面標題 |
| `StatCard` | shared/StatCard.tsx | 統計卡片 |
| `LoadingSpinner` | shared/LoadingSpinner.tsx | 載入動畫 |
| `ConfirmDialog` | shared/ConfirmDialog.tsx | 確認對話框 |
| `SpeechToText` | shared/SpeechToText.tsx | 語音輸入 |
| `AIOptimizeButton` | shared/AIOptimizeButton.tsx | AI 優化按鈕 |

---

## 7. MVC 用戶前台

### 7.1 控制器說明

| 控制器 | 路由前綴 | 功能 |
|--------|----------|------|
| HomeController | `/` | 首頁、組織介紹、聯絡方式 |
| AuthController | `/Auth` | 登入、註冊、密碼重設 |
| UserController | `/User` | 用戶個人資料、報名紀錄 |
| CaseController | `/Case` | 個案相關功能 |
| ActivityController | `/Activity` | 活動列表、報名 |
| PurchaseController | `/Purchase` | 購物、付款、訂單 |
| EventController | `/Event` | 活動事件 |
| DashboardController | `/Dashboard` | 用戶儀表板 |

### 7.2 ECPay 金流整合

```csharp
// 付款流程
1. 用戶選擇商品 → /Purchase/Index
2. 結帳建立訂單 → /Purchase/Payment
3. 重導至 ECPay → EcpayRedirect.cshtml (自動提交表單)
4. ECPay 處理付款
5. 伺服器回調 → /Purchase/EcpayReturn (驗證簽章、更新訂單)
6. 用戶返回 → /Purchase/Success

// CheckMacValue 計算 (SHA256)
string GenerateCheckMacValue(Dictionary<string, string> parameters)
{
    // 1. 參數按 key 排序
    // 2. 串接成 key=value&key=value
    // 3. 前後加上 HashKey 和 HashIV
    // 4. URL Encode (小寫)
    // 5. SHA256 加密 (大寫)
}
```

### 7.3 Google OAuth 登入

```csharp
// Program.cs 配置
builder.Services.AddAuthentication()
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
        options.SaveTokens = true;
    });

// 登入流程
1. 用戶點擊 "Google 登入"
2. 重導至 Google OAuth
3. 用戶授權
4. 回調 /Auth/GoogleCallback
5. 建立 Session / 導向首頁
```

---

## 8. 資料庫設計

### 8.1 資料庫資訊

- **名稱**: NGOPlatformDb
- **引擎**: SQL Server 2019+
- **連線**: `Server=YUNYUE\SQLEXPRESS;Database=NGOPlatformDb;...`

### 8.2 資料表關聯圖

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Workers   │     │    Cases    │     │  Activities │
│─────────────│     │─────────────│     │─────────────│
│ WorkerId PK │◄────│ WorkerId FK │     │ ActivityId  │
│ Email       │     │ CaseId PK   │     │ WorkerId FK │
│ Password    │     │ Name        │     │ Name        │
│ Role        │     │ Phone       │     │ Status      │
└─────────────┘     │ IdentityNum │     └──────┬──────┘
                    │ Status      │            │
                    └──────┬──────┘            │
                           │                   │
              ┌────────────┴────────────┐      │
              │                         │      │
       ┌──────┴──────┐          ┌───────┴──────┴───────┐
       │ CaseOrders  │          │CaseActivityRegistration│
       │─────────────│          │─────────────────────────│
       │ OrderId PK  │          │ RegistrationId PK       │
       │ CaseId FK   │          │ CaseId FK               │
       │ SupplyId FK │          │ ActivityId FK           │
       │ Quantity    │          │ Status                  │
       └──────┬──────┘          └─────────────────────────┘
              │
       ┌──────┴──────┐
       │  Supplies   │
       │─────────────│
       │ SupplyId PK │
       │ Name        │
       │ Price       │
       │ Quantity    │
       └─────────────┘
```

### 8.3 主要資料表

| 表名 | 說明 | 主要欄位 |
|------|------|----------|
| Workers | 員工/志工 | WorkerId, Email, Password, Role |
| Cases | 個案 | CaseId, Name, Phone, IdentityNumber, WorkerId |
| Activities | 活動 | ActivityId, Name, StartDate, MaxParticipants |
| Supplies | 物資 | SupplyId, Name, Price, Quantity |
| Users | 民眾使用者 | UserId, Email, Password, Phone |
| UserOrders | 用戶訂單 | OrderId, UserId, TotalPrice, PaymentStatus |
| CaseActivityRegistration | 個案活動報名 | CaseId, ActivityId, Status |
| UserActivityRegistration | 民眾活動報名 | UserId, ActivityId, Status |
| Schedule | 行程 | ScheduleId, WorkerId, CaseId, DateTime |

---

## 9. 認證與安全

### 9.1 JWT Token 認證 (WebAPI)

```csharp
// 配置
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Token 結構
{
  "sub": "user_id",
  "email": "user@example.com",
  "role": "Admin",
  "exp": 1234567890
}
```

### 9.2 密碼加密 (Argon2)

```csharp
public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = RandomNumberGenerator.GetBytes(16);
        argon2.DegreeOfParallelism = 4;
        argon2.MemorySize = 65536;  // 64 MB
        argon2.Iterations = 3;

        var hash = argon2.GetBytes(32);
        // 返回 Base64 編碼的 salt + hash
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // 驗證密碼
    }
}
```

### 9.3 輸入驗證 (FluentValidation)

```csharp
public class CreateCaseDtoValidator : AbstractValidator<CreateCaseDto>
{
    public CreateCaseDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("姓名不能為空")
            .MaximumLength(50);

        RuleFor(x => x.IdentityNumber)
            .NotEmpty()
            .Matches(@"^[A-Z][12][0-9]{8}$")
            .WithMessage("身分證格式錯誤");

        RuleFor(x => x.Phone)
            .Matches(@"^09\d{8}$")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

### 9.4 CORS 配置

```csharp
// 開發環境
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 生產環境
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
            "https://admin.ngo-management-hub.com",
            "https://ngo-management-hub.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

---

## 10. 外部服務整合

### 10.1 Azure 服務

| 服務 | 用途 | 配置 Key |
|------|------|----------|
| Azure OpenAI (GPT-4) | 活動描述 AI 優化 | AzureOpenAI:Endpoint, ApiKey |
| Azure OpenAI (DALL-E 3) | 活動圖片 AI 生成 | AzureOpenAI:DalleDeploymentName |
| Azure Speech Service | 語音轉文字 | AzureSpeech:Key, Region |
| Azure Blob Storage | 圖片/檔案儲存 | AzureStorage:ConnectionString |

### 10.2 Google 服務

| 服務 | 用途 | 配置 Key |
|------|------|----------|
| Google OAuth 2.0 | 第三方登入 | Authentication:Google:ClientId |
| Google Maps API | 地圖選擇器 | VITE_GOOGLE_MAPS_API_KEY |

### 10.3 ECPay 綠界金流

| 項目 | 說明 |
|------|------|
| 服務類型 | 線上信用卡付款 |
| 簽章方式 | SHA256 CheckMacValue |
| 測試卡號 | 4311-9511-1111-1111 |
| 測試到期日 | 任意未來日期 |
| 測試 CVV | 222 |

---

## 11. 部署配置

### 11.1 本地開發環境

```bash
# 終端機 1 - 後端 API
cd api
dotnet run
# http://localhost:5264

# 終端機 2 - React 前端
cd react-admin
npm install
npm run dev
# http://localhost:5173

# 終端機 3 - MVC 前端
cd dotnet-web/NGOPlatformWeb
dotnet run
# http://localhost:5066
```

### 11.2 環境變數

#### 後端 (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YUNYUE\\SQLEXPRESS;Database=NGOPlatformDb;..."
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "NGO-Platform",
    "Audience": "NGO-Platform-Users"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4",
    "DalleDeploymentName": "dall-e-3"
  }
}
```

#### 前端 (.env.development)

```env
VITE_API_BASE_URL=http://localhost:5264/api
VITE_APP_NAME=NGO案管系統
VITE_ENABLE_AZURE_LOGIN=false
VITE_GOOGLE_MAPS_API_KEY=your-api-key
```

### 11.3 Docker 部署

```dockerfile
# 後端 Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "NGO_WebAPI_Backend.dll"]
```

### 11.4 Cloudflare Tunnel (公開展示)

| 服務 | 域名 |
|------|------|
| 後端 API | https://api.ngo-management-hub.com |
| React 管理後台 | https://admin.ngo-management-hub.com |
| MVC 用戶前台 | https://ngo-management-hub.com |

---

## 12. 開發指南

### 12.1 新增 API 端點

1. **建立 Controller**
```csharp
[ApiController]
[Route("api/[controller]")]
public class NewController : ControllerBase
{
    private readonly INewService _service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NewDto>>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<NewDto>>.Success(result));
    }
}
```

2. **建立 Service**
```csharp
public interface INewService
{
    Task<List<NewDto>> GetAllAsync();
}

public class NewService : INewService
{
    private readonly INewRepository _repository;

    public async Task<List<NewDto>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
}
```

3. **註冊 DI**
```csharp
// Program.cs
builder.Services.AddScoped<INewService, NewService>();
builder.Services.AddScoped<INewRepository, NewRepository>();
```

### 12.2 新增前端頁面

1. **建立頁面組件**
```typescript
// src/pages/NewPage.tsx
export default function NewPage() {
  return (
    <PageContainer>
      <PageHeader title="新頁面" />
      {/* 內容 */}
    </PageContainer>
  );
}
```

2. **新增路由**
```typescript
// src/routes/index.tsx
{
  path: 'new-page',
  element: <NewPage />
}
```

3. **新增側邊欄項目**
```typescript
// src/components/layout/Sidebar.tsx
{ text: '新頁面', icon: <NewIcon />, path: '/new-page' }
```

### 12.3 測試帳號

| 系統 | Email | 密碼 | 角色 |
|------|-------|------|------|
| React 管理後台 | admin@ngo.org | Admin123! | 管理員 |
| React 管理後台 | supervisor@ngo.org | Super123! | 督導 |
| React 管理後台 | staff@ngo.org | Staff123! | 員工 |
| MVC 用戶前台 | test@example.com | Test123! | 用戶 |

---

## 附錄

### A. 常用指令

```bash
# Git
git status
git add .
git commit -m "message"
git push

# .NET
dotnet build
dotnet run
dotnet ef migrations add MigrationName
dotnet ef database update

# npm
npm install
npm run dev
npm run build
npm run preview
```

### B. 相關文件

- `/docs/00-快速開始.md` - 快速啟動指南
- `/docs/01-系統架構與功能說明.md` - 詳細架構文件
- `/docs/02-Cloudflare展示指南.md` - Tunnel 設定
- `/docs/03-測試帳號.md` - 測試帳號資訊
- `/docs/04-ECPay綠界金流技術分析.md` - ECPay 技術說明

### C. 聯絡資訊

如有任何問題，請參考專案文件或提交 Issue。

---

**文件版本**: 1.0.0
**建立日期**: 2025-01-29
**維護者**: NGO Management System Team
