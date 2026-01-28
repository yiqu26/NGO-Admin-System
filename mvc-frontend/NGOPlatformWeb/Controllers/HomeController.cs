using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NGOPlatformWeb.Models;
using NGOPlatformWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NGOPlatformWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly NGODbContext _context;

        public HomeController(NGODbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var model = new HomeViewModel
                {
                    // 輪播圖資料
                    CarouselItems = GetCarouselItems(),
                    
                    // 最新活動（根據登入狀態顯示不同活動）
                    RecentActivities = GetRecentActivities(),
                    
                    // 組織影響力統計
                    ImpactStats = GetImpactStats(),
                    
                    // 使命陳述
                    MissionStatement = GetMissionStatement()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // 記錄詳細錯誤信息
                ViewBag.ErrorMessage = $"首頁載入發生錯誤: {ex.Message}";
                ViewBag.ErrorDetail = ex.ToString();
                
                // 回傳一個空的模型，避免頁面完全無法顯示
                var emptyModel = new HomeViewModel();
                return View(emptyModel);
            }
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "這裡是聯絡我們";
            return View(); // 聯絡我們
        }

        public IActionResult Organization()
        {
            ViewData["Title"] = "這裡是組織介紹";
            return View(); // 組織介紹
        }
        // 輪播圖資料
        private List<CarouselItem> GetCarouselItems()
        {
            return new List<CarouselItem>
            {
                new CarouselItem
                {
                    ImageUrl = "/images/homepageC1.png",
                    Title = "建立更美好的社會",
                    Description = "我們致力於連結資源與需求，透過專業服務與社區參與，為弱勢族群提供實質幫助",
                    ButtonText = "立即加入",
                    ButtonLink = "/Auth/Register"
                },
                new CarouselItem
                {
                    ImageUrl = "/images/homepageC2.png",
                    Title = "志工招募進行中",
                    Description = "成為我們的志工，一起為社會帶來正面影響，用愛心與行動改變世界",
                    ButtonText = "成為志工",
                    ButtonLink = "/Auth/Register"
                },
                new CarouselItem
                {
                    ImageUrl = "/images/homepageC3.png",
                    Title = "社區服務活動",
                    Description = "參與我們的社區服務活動，與鄰里一起創造溫暖的生活環境",
                    ButtonText = "查看活動",
                    ButtonLink = "/Activity/Index"
                }
            };
        }

        // 根據登入狀態取得最新活動 - 智慧顯示不同角色的活動
        private List<NGOPlatformWeb.Models.Entity.Activity> GetRecentActivities()
        {
            var targetAudience = "public"; // 預設顯示一般民眾活動
            
            // 檢查是否為個案登入，個案只看到個案專屬活動
            if (User.Identity.IsAuthenticated)
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (userRole == "Case")
                {
                    targetAudience = "case";
                }
            }
            
            // 只顯示未來時間且開放報名的活動，最多3個
            return _context.Activities
                .Where(a => a.TargetAudience == targetAudience && a.Status == "open" && a.StartDate > DateTime.Now)
                .OrderBy(a => a.StartDate)
                .Take(3)
                .ToList();
        }
        
        // 即時計算組織影響力統計 - 為首頁展示提供數據
        private ImpactStats GetImpactStats()
        {
            var totalActivities = _context.Activities.Count();
            var totalParticipants = _context.Activities.Sum(a => a.CurrentParticipants);
            var totalCases = _context.Cases.Count();
            var totalSupplies = _context.Supplies.Count();
            
            return new ImpactStats
            {
                TotalActivities = totalActivities,
                TotalParticipants = totalParticipants,
                TotalCases = totalCases,
                TotalSupplies = totalSupplies
            };
        }
        
        // 使命陳述
        private MissionStatement GetMissionStatement()
        {
            return new MissionStatement
            {
                Title = "建立更美好的社會",
                Description = "我們致力於連結資源與需求，透過專業服務與社區參與，為弱勢族群提供實質幫助，共同創造充滿希望的未來。",
                ImageUrl = "/images/activityback.jpg"
            };
        }

    }
}
