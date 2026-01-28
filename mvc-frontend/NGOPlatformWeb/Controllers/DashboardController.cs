using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Dashboard;

namespace NGOPlatformWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly NGODbContext _context;

        public DashboardController(NGODbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUserDashboardData(int userId)
        {
            // 目前不需要圖表，但保留API以備未來使用
            return Json(new { message = "Dashboard API ready for future use" });
        }

        private List<MonthlyDonationData> GetMonthlyDonationsFromRealData(List<UserOrder> orders)
        {
            var now = DateTime.Now;
            var monthlyData = new List<MonthlyDonationData>();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var monthOrders = orders.Where(o => 
                    o.OrderDate.Year == targetMonth.Year && 
                    o.OrderDate.Month == targetMonth.Month).ToList();

                monthlyData.Add(new MonthlyDonationData
                {
                    Month = targetMonth.ToString("yyyy/MM"),
                    // 由於真實數據中沒有OrderSource，先用TotalPrice分組
                    EmergencyAmount = monthOrders.Where(o => o.TotalPrice > 500).Sum(o => o.TotalPrice), // 假設高金額為緊急援助
                    PackageAmount = monthOrders.Where(o => o.TotalPrice <= 500).Sum(o => o.TotalPrice), // 假設低金額為組合包
                    TotalOrders = monthOrders.Count
                });
            }

            return monthlyData;
        }

        // 保留舊方法以防萬一
        private List<MonthlyDonationData> GetMonthlyDonations(List<UserOrder> orders)
        {
            return GetMonthlyDonationsFromRealData(orders);
        }

        private List<CategoryDistributionData> GetCategoryDistributionSimple(List<UserOrderDetail> orderDetails)
        {
            if (!orderDetails.Any())
            {
                return GetMockCategoryData();
            }

            // 簡化版本，按金額範圍分類
            var totalAmount = orderDetails.Sum(d => d.UnitPrice * d.Quantity);
            
            var categoryData = new List<CategoryDistributionData>
            {
                new CategoryDistributionData
                {
                    CategoryName = "食品飲料",
                    TotalAmount = totalAmount * 0.4m,
                    OrderCount = orderDetails.Count(d => d.UnitPrice <= 50),
                    Percentage = 40.0
                },
                new CategoryDistributionData
                {
                    CategoryName = "服裝衣物",
                    TotalAmount = totalAmount * 0.25m,
                    OrderCount = orderDetails.Count(d => d.UnitPrice > 50 && d.UnitPrice <= 200),
                    Percentage = 25.0
                },
                new CategoryDistributionData
                {
                    CategoryName = "醫療健康",
                    TotalAmount = totalAmount * 0.2m,
                    OrderCount = orderDetails.Count(d => d.UnitPrice > 200),
                    Percentage = 20.0
                },
                new CategoryDistributionData
                {
                    CategoryName = "清潔用品",
                    TotalAmount = totalAmount * 0.15m,
                    OrderCount = orderDetails.Count(d => d.Quantity > 5),
                    Percentage = 15.0
                }
            };

            return categoryData;
        }

        private List<CategoryDistributionData> GetCategoryDistribution(List<UserOrder> orders)
        {
            var orderDetails = orders.SelectMany(o => o.OrderDetails).ToList();
            return GetCategoryDistributionSimple(orderDetails);
        }

        private DonationTrendData GetDonationTrendsFromRealData(List<UserOrder> orders)
        {
            var orderedByDate = orders.OrderBy(o => o.OrderDate).ToList();
            var recentOrders = orders.Where(o => o.OrderDate >= DateTime.Now.AddDays(-30)).ToList();
            
            return new DonationTrendData
            {
                AverageDonationAmount = orders.Any() ? Math.Round(orders.Average(o => o.TotalPrice), 0) : 0,
                LargestDonation = orders.Any() ? orders.Max(o => o.TotalPrice) : 0,
                DonationFrequency = CalculateDonationFrequency(orders),
                GrowthRate = CalculateGrowthRate(orders),
                RecentActivityDays = recentOrders.Any() ? 
                    (DateTime.Now - recentOrders.Max(o => o.OrderDate)).Days : 0
            };
        }

        private DonationTrendData GetDonationTrends(List<UserOrder> orders)
        {
            return GetDonationTrendsFromRealData(orders);
        }

        private ImpactStatisticsData GetImpactStatisticsSimple(List<UserOrder> orders, List<UserOrderDetail> orderDetails)
        {
            if (!orders.Any())
            {
                return GetMockImpactData();
            }
            
            var totalItems = orderDetails.Sum(d => d.Quantity);
            
            // 從數據結構分析：由於沒有OrderSource，使用金額判斷緊急援助
            var emergencyOrders = orders.Where(o => o.TotalPrice > 1000).Count();
            
            // 簡化版本：基於現有數據估算
            var categoriesHelped = Math.Min(orderDetails.Select(d => d.SupplyId).Distinct().Count() / 5, 5);

            return new ImpactStatisticsData
            {
                TotalItemsDonated = totalItems,
                EmergencyResponseCount = emergencyOrders,
                CategoriesHelped = categoriesHelped,
                ConsistencyScore = CalculateConsistencyScore(orders)
            };
        }

        private ImpactStatisticsData GetImpactStatistics(List<UserOrder> orders)
        {
            var orderDetails = orders.SelectMany(o => o.OrderDetails).ToList();
            return GetImpactStatisticsSimple(orders, orderDetails);
        }

        private string CalculateDonationFrequency(List<UserOrder> orders)
        {
            if (orders.Count < 2) return "新手捐助者";
            
            var daysBetween = (orders.Max(o => o.OrderDate) - orders.Min(o => o.OrderDate)).Days;
            var avgDaysBetween = daysBetween / (double)(orders.Count - 1);
            
            if (avgDaysBetween <= 7) return "週週行善";
            if (avgDaysBetween <= 30) return "月月送暖";
            if (avgDaysBetween <= 90) return "季季關懷";
            return "年度善心";
        }

        private double CalculateGrowthRate(List<UserOrder> orders)
        {
            if (orders.Count < 2) return 0;
            
            var orderedByDate = orders.OrderBy(o => o.OrderDate).ToList();
            var firstHalf = orderedByDate.Take(orderedByDate.Count / 2).Sum(o => o.TotalPrice);
            var secondHalf = orderedByDate.Skip(orderedByDate.Count / 2).Sum(o => o.TotalPrice);
            
            if (firstHalf == 0) return 0;
            return Math.Round((double)(((secondHalf - firstHalf) / firstHalf) * 100), 1);
        }

        private int CalculateConsistencyScore(List<UserOrder> orders)
        {
            if (orders.Count == 0) return 0;
            
            var score = 0;
            if (orders.Count >= 3) score += 20;
            // 調整：使用金額判斷緊急援助參與
            if (orders.Any(o => o.TotalPrice > 1000)) score += 30;
            if (orders.Count >= 5) score += 25;
            if (orders.Sum(o => o.TotalPrice) >= 5000) score += 25;
            
            return Math.Min(score, 100);
        }

        private UserDashboardViewModel GetMockDashboardData()
        {
            return new UserDashboardViewModel
            {
                MonthlyDonations = GetMockMonthlyData(),
                CategoryDistribution = GetMockCategoryData(),
                DonationTrends = GetMockTrendData(),
                ImpactStatistics = GetMockImpactData()
            };
        }

        private List<MonthlyDonationData> GetMockMonthlyData()
        {
            return new List<MonthlyDonationData>
            {
                new MonthlyDonationData { Month = "2025/02", EmergencyAmount = 320, PackageAmount = 280, TotalOrders = 2 },
                new MonthlyDonationData { Month = "2025/03", EmergencyAmount = 150, PackageAmount = 420, TotalOrders = 3 },
                new MonthlyDonationData { Month = "2025/04", EmergencyAmount = 680, PackageAmount = 180, TotalOrders = 2 },
                new MonthlyDonationData { Month = "2025/05", EmergencyAmount = 210, PackageAmount = 350, TotalOrders = 3 },
                new MonthlyDonationData { Month = "2025/06", EmergencyAmount = 450, PackageAmount = 280, TotalOrders = 2 },
                new MonthlyDonationData { Month = "2025/07", EmergencyAmount = 850, PackageAmount = 415, TotalOrders = 4 }
            };
        }

        private List<CategoryDistributionData> GetMockCategoryData()
        {
            return new List<CategoryDistributionData>
            {
                new CategoryDistributionData { CategoryName = "食品飲料", TotalAmount = 1680, OrderCount = 8, Percentage = 42.3 },
                new CategoryDistributionData { CategoryName = "生活用品", TotalAmount = 980, OrderCount = 4, Percentage = 24.7 },
                new CategoryDistributionData { CategoryName = "醫療健康", TotalAmount = 680, OrderCount = 3, Percentage = 17.1 },
                new CategoryDistributionData { CategoryName = "服裝衣物", TotalAmount = 420, OrderCount = 2, Percentage = 10.6 },
                new CategoryDistributionData { CategoryName = "學習用品", TotalAmount = 210, OrderCount = 1, Percentage = 5.3 }
            };
        }

        private DonationTrendData GetMockTrendData()
        {
            return new DonationTrendData
            {
                AverageDonationAmount = 380,
                LargestDonation = 850,
                DonationFrequency = "穩定善心",
                GrowthRate = 8.2,
                RecentActivityDays = 7
            };
        }

        private ImpactStatisticsData GetMockImpactData()
        {
            return new ImpactStatisticsData
            {
                TotalItemsDonated = 97,
                EmergencyResponseCount = 3,
                CategoriesHelped = 5,
                ConsistencyScore = 72
            };
        }
    }
}