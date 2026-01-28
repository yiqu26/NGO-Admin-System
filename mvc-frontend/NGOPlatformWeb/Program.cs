using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Services;


var builder = WebApplication.CreateBuilder(args);

// Service registration
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // 與認證 Cookie 一致
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // 允許跨域時保持 Cookie，改為 None 以支援 Ngrok
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 必須使用 Secure 當 SameSite=None
});

// Add Data Protection for OAuth state management
builder.Services.AddDataProtection();
// Add Service Dependent Injection
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<PasswordMigrationService>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<EcpayService>();
builder.Services.AddScoped<AchievementService>();
// Activity Service Layer
builder.Services.AddScoped<NGOPlatformWeb.Repositories.IActivityRepository, NGOPlatformWeb.Repositories.ActivityRepository>();
builder.Services.AddScoped<NGOPlatformWeb.Services.IActivityService, NGOPlatformWeb.Services.ActivityService>();
// Add Background Service
builder.Services.AddHostedService<TokenCleanupService>();
// DbContext
builder.Services.AddDbContext<NGODbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NGODb")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // 延長到 2 小時，足夠完成付款流程
        options.SlidingExpiration = true; // 啟用滑動過期（每次活動重置時間）
        
        // 修復跨域付款時的 Cookie 設定
        options.Cookie.SameSite = SameSiteMode.None; // 允許跨域時保持 Cookie，改為 None 以支援 Ngrok
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 必須使用 Secure 當 SameSite=None
        options.Cookie.HttpOnly = true; // 防止 XSS 攻擊
        options.Cookie.IsEssential = true; // 確保 Cookie 不被 GDPR 政策阻擋
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        // 明確請求 profile scope 以獲取用戶頭像和其他資訊
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        
        // 明確映射 Google 的 picture claim（官方文檔標準做法）
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
        
        // 確保獲取完整的用戶資訊
        options.SaveTokens = true;
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
