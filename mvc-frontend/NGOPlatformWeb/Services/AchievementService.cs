using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGOPlatformWeb.Services
{
    public class AchievementService
    {
        private readonly NGODbContext _context;
        
        public AchievementService(NGODbContext context)
        {
            _context = context;
        }

        // 成就定義 - 基於ER圖完整設計
        public static readonly Dictionary<string, AchievementDefinition> ACHIEVEMENTS = new()
        {
            // === 活動參與成就 ===
            ["first_registration"] = new AchievementDefinition
            {
                Code = "first_registration",
                Name = "初來乍到",
                Description = "完成第一次活動報名",
                Icon = "star",
                Category = "activity",
                Tier = 1
            },
            ["first_completion"] = new AchievementDefinition
            {
                Code = "first_completion", 
                Name = "初體驗",
                Description = "完成第一次活動參與",
                Icon = "check-circle",
                Category = "activity",
                Tier = 1
            },
            ["multi_category"] = new AchievementDefinition
            {
                Code = "multi_category",
                Name = "多元探索者", 
                Description = "參與3種不同類型的活動",
                Icon = "rainbow",
                Category = "activity",
                Tier = 2
            },
            ["regular_participant"] = new AchievementDefinition
            {
                Code = "regular_participant",
                Name = "熱心志工",
                Description = "累計完成5次活動",
                Icon = "heart",
                Category = "activity",
                Tier = 2
            },
            ["case_helper"] = new AchievementDefinition
            {
                Code = "case_helper",
                Name = "溫暖陪伴",
                Description = "參與個案專屬活動", 
                Icon = "users",
                Category = "activity",
                Tier = 2
            },
            ["activity_veteran"] = new AchievementDefinition
            {
                Code = "activity_veteran",
                Name = "資深志工",
                Description = "累計完成10次活動",
                Icon = "award",
                Category = "activity",
                Tier = 3
            },
            ["monthly_active"] = new AchievementDefinition
            {
                Code = "monthly_active",
                Name = "月度達人",
                Description = "單月內參與3次以上活動",
                Icon = "calendar-check",
                Category = "activity",
                Tier = 2
            },

            // === 認購捐贈成就 ===
            ["first_purchase"] = new AchievementDefinition
            {
                Code = "first_purchase",
                Name = "愛心初心",
                Description = "完成第一次物資認購",
                Icon = "gift",
                Category = "purchase",
                Tier = 1
            },
            ["purchase_supporter"] = new AchievementDefinition
            {
                Code = "purchase_supporter",
                Name = "物資支持者",
                Description = "累計認購5次物資",
                Icon = "hand-holding-heart",
                Category = "purchase",
                Tier = 2
            },
            ["generous_donor"] = new AchievementDefinition
            {
                Code = "generous_donor",
                Name = "慷慨捐助",
                Description = "單筆認購金額達NT$1000",
                Icon = "donate",
                Category = "purchase",
                Tier = 2
            },
            ["emergency_hero"] = new AchievementDefinition
            {
                Code = "emergency_hero",
                Name = "急難英雄",
                Description = "認購緊急物資需求",
                Icon = "ambulance",
                Category = "purchase",
                Tier = 3
            },
            ["supply_diversity"] = new AchievementDefinition
            {
                Code = "supply_diversity",
                Name = "全方位關懷",
                Description = "認購5種不同類別的物資",
                Icon = "boxes",
                Category = "purchase",
                Tier = 3
            },
            ["big_supporter"] = new AchievementDefinition
            {
                Code = "big_supporter",
                Name = "重量級支持者",
                Description = "累計認購金額達NT$5000",
                Icon = "medal",
                Category = "purchase",
                Tier = 4
            },
            ["monthly_donor"] = new AchievementDefinition
            {
                Code = "monthly_donor",
                Name = "月度捐贈者",
                Description = "連續3個月都有認購記錄",
                Icon = "clock",
                Category = "purchase",
                Tier = 3
            },

            // === 綜合成就 ===
            ["well_rounded"] = new AchievementDefinition
            {
                Code = "well_rounded",
                Name = "全方位志工",
                Description = "同時參與活動和認購物資",
                Icon = "balance-scale",
                Category = "comprehensive",
                Tier = 3
            },
            ["platform_ambassador"] = new AchievementDefinition
            {
                Code = "platform_ambassador",
                Name = "平台大使",
                Description = "累計參與10次活動且認購滿NT$3000",
                Icon = "crown",
                Category = "comprehensive",
                Tier = 4
            },
            ["loyalty_member"] = new AchievementDefinition
            {
                Code = "loyalty_member",
                Name = "忠實會員",
                Description = "註冊滿6個月且持續參與",
                Icon = "certificate",
                Category = "comprehensive",
                Tier = 3
            }
        };

        /// <summary>
        /// 檢查並獎勵新成就
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>新獲得的成就代碼列表</returns>
        public async Task<List<string>> CheckAndAwardAchievements(int userId)
        {
            var newAchievements = new List<string>();
            
            // 獲取用戶已有的成就
            var existingAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementCode)
                .ToListAsync();

            // 檢查每個成就條件
            foreach (var achievement in ACHIEVEMENTS.Values)
            {
                if (!existingAchievements.Contains(achievement.Code) && 
                    await MeetsAchievementCondition(userId, achievement.Code))
                {
                    await AwardAchievement(userId, achievement.Code);
                    newAchievements.Add(achievement.Code);
                }
            }

            return newAchievements;
        }

        /// <summary>
        /// 獲取用戶所有成就 - 支援分類排版
        /// </summary>
        public async Task<List<UserAchievementViewModel>> GetUserAchievements(int userId)
        {
            var earnedAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            var result = new List<UserAchievementViewModel>();

            foreach (var achievement in ACHIEVEMENTS.Values)
            {
                var earned = earnedAchievements.FirstOrDefault(ea => ea.AchievementCode == achievement.Code);
                
                result.Add(new UserAchievementViewModel
                {
                    Code = achievement.Code,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    Icon = achievement.Icon,
                    Category = achievement.Category,
                    Tier = achievement.Tier,
                    IsEarned = earned != null,
                    EarnedAt = earned?.EarnedAt,
                    IsNew = earned != null && earned.EarnedAt > DateTime.Now.AddHours(-24) // 24小時內獲得算新的
                });
            }

            // 按分類、等級、獲得狀態排序
            return result
                .OrderBy(a => a.Category == "activity" ? 1 : a.Category == "purchase" ? 2 : 3) // 分類順序
                .ThenBy(a => a.Tier) // 難度等級
                .ThenByDescending(a => a.IsEarned) // 已獲得優先
                .ThenBy(a => a.Name) // 名稱排序
                .ToList();
        }

        /// <summary>
        /// 取得分類統計資訊
        /// </summary>
        public async Task<Dictionary<string, AchievementCategoryStats>> GetAchievementCategoryStats(int userId)
        {
            var userAchievements = await GetUserAchievements(userId);
            var stats = new Dictionary<string, AchievementCategoryStats>();

            var categories = new[] { "activity", "purchase", "comprehensive" };
            var categoryNames = new Dictionary<string, string>
            {
                ["activity"] = "活動參與",
                ["purchase"] = "愛心認購", 
                ["comprehensive"] = "綜合成就"
            };

            foreach (var category in categories)
            {
                var categoryAchievements = userAchievements.Where(a => a.Category == category).ToList();
                stats[category] = new AchievementCategoryStats
                {
                    CategoryName = categoryNames[category],
                    TotalCount = categoryAchievements.Count,
                    EarnedCount = categoryAchievements.Count(a => a.IsEarned),
                    NewCount = categoryAchievements.Count(a => a.IsNew)
                };
            }

            return stats;
        }

        /// <summary>
        /// 檢查是否滿足成就條件
        /// </summary>
        private async Task<bool> MeetsAchievementCondition(int userId, string achievementCode)
        {
            return achievementCode switch
            {
                // 活動相關成就
                "first_registration" => await HasFirstRegistration(userId),
                "first_completion" => await HasFirstCompletion(userId),
                "multi_category" => await HasMultiCategory(userId),
                "regular_participant" => await HasRegularParticipation(userId),
                "case_helper" => await HasCaseHelperActivity(userId),
                "activity_veteran" => await HasActivityVeteran(userId),
                "monthly_active" => await HasMonthlyActive(userId),
                
                // 認購相關成就
                "first_purchase" => await HasFirstPurchase(userId),
                "purchase_supporter" => await HasPurchaseSupporter(userId),
                "generous_donor" => await HasGenerousDonor(userId),
                "emergency_hero" => await HasEmergencyHero(userId),
                "supply_diversity" => await HasSupplyDiversity(userId),
                "big_supporter" => await HasBigSupporter(userId),
                "monthly_donor" => await HasMonthlyDonor(userId),
                
                // 綜合成就
                "well_rounded" => await HasWellRounded(userId),
                "platform_ambassador" => await HasPlatformAmbassador(userId),
                "loyalty_member" => await HasLoyaltyMember(userId),
                
                _ => false
            };
        }

        /// <summary>
        /// 獎勵成就
        /// </summary>
        private async Task AwardAchievement(int userId, string achievementCode)
        {
            var userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementCode = achievementCode,
                EarnedAt = DateTime.Now
            };

            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();
        }

        // === 活動相關成就檢查方法 ===
        private async Task<bool> HasFirstRegistration(int userId)
        {
            return await _context.UserActivityRegistrations
                .AnyAsync(uar => uar.UserId == userId);
        }

        private async Task<bool> HasFirstCompletion(int userId)
        {
            return await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .AnyAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
        }

        private async Task<bool> HasMultiCategory(int userId)
        {
            var categories = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a.Category)
                .Distinct()
                .CountAsync();
            
            return categories >= 3;
        }

        private async Task<bool> HasRegularParticipation(int userId)
        {
            var completedCount = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .CountAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
            
            return completedCount >= 5;
        }

        private async Task<bool> HasCaseHelperActivity(int userId)
        {
            return await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .AnyAsync(a => a.TargetAudience == "case");
        }

        private async Task<bool> HasActivityVeteran(int userId)
        {
            var completedCount = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .CountAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
            
            return completedCount >= 10;
        }

        private async Task<bool> HasMonthlyActive(int userId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            var monthlyCount = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && 
                             uar.RegisterTime.Month == currentMonth && 
                             uar.RegisterTime.Year == currentYear)
                .CountAsync();
            
            return monthlyCount >= 3;
        }

        // === 認購相關成就檢查方法 ===
        private async Task<bool> HasFirstPurchase(int userId)
        {
            return await _context.UserOrders
                .AnyAsync(uo => uo.UserId == userId && uo.PaymentStatus == "已付款");
        }

        private async Task<bool> HasPurchaseSupporter(int userId)
        {
            var purchaseCount = await _context.UserOrders
                .Where(uo => uo.UserId == userId && uo.PaymentStatus == "已付款")
                .CountAsync();
            
            return purchaseCount >= 5;
        }

        private async Task<bool> HasGenerousDonor(int userId)
        {
            return await _context.UserOrders
                .AnyAsync(uo => uo.UserId == userId && uo.PaymentStatus == "已付款" && uo.TotalPrice >= 1000);
        }

        private async Task<bool> HasEmergencyHero(int userId)
        {
            return await _context.EmergencyPurchaseRecords
                .Join(_context.UserOrders,
                    epr => epr.UserOrderId,
                    uo => uo.UserOrderId,
                    (epr, uo) => uo)
                .AnyAsync(uo => uo.UserId == userId && uo.PaymentStatus == "已付款");
        }

        private async Task<bool> HasSupplyDiversity(int userId)
        {
            var categoryCount = await _context.UserOrderDetails
                .Join(_context.UserOrders,
                    uod => uod.UserOrderId,
                    uo => uo.UserOrderId,
                    (uod, uo) => new { uod, uo })
                .Where(x => x.uo.UserId == userId && x.uo.PaymentStatus == "已付款")
                .Join(_context.Supplies,
                    x => x.uod.SupplyId,
                    s => s.SupplyId,
                    (x, s) => s)
                .Join(_context.SupplyCategories,
                    s => s.SupplyCategoryId,
                    sc => sc.SupplyCategoryId,
                    (s, sc) => sc.SupplyCategoryName)
                .Distinct()
                .CountAsync();
            
            return categoryCount >= 5;
        }

        private async Task<bool> HasBigSupporter(int userId)
        {
            var totalAmount = await _context.UserOrders
                .Where(uo => uo.UserId == userId && uo.PaymentStatus == "已付款")
                .SumAsync(uo => uo.TotalPrice);
            
            return totalAmount >= 5000;
        }

        private async Task<bool> HasMonthlyDonor(int userId)
        {
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var monthlyPurchases = await _context.UserOrders
                .Where(uo => uo.UserId == userId && 
                            uo.PaymentStatus == "已付款" && 
                            uo.OrderDate >= threeMonthsAgo)
                .GroupBy(uo => new { uo.OrderDate.Year, uo.OrderDate.Month })
                .CountAsync();
            
            return monthlyPurchases >= 3;
        }

        // === 綜合成就檢查方法 ===
        private async Task<bool> HasWellRounded(int userId)
        {
            var hasActivity = await _context.UserActivityRegistrations
                .AnyAsync(uar => uar.UserId == userId);
            
            var hasPurchase = await _context.UserOrders
                .AnyAsync(uo => uo.UserId == userId && uo.PaymentStatus == "已付款");
            
            return hasActivity && hasPurchase;
        }

        private async Task<bool> HasPlatformAmbassador(int userId)
        {
            var activityCount = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId && uar.Status == "registered")
                .Join(_context.Activities,
                    uar => uar.ActivityId,
                    a => a.ActivityId,
                    (uar, a) => a)
                .CountAsync(a => a.Status == "completed" && a.EndDate < DateTime.Now);
            
            var totalAmount = await _context.UserOrders
                .Where(uo => uo.UserId == userId && uo.PaymentStatus == "已付款")
                .SumAsync(uo => uo.TotalPrice);
            
            return activityCount >= 10 && totalAmount >= 3000;
        }

        private async Task<bool> HasLoyaltyMember(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;
            
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            // 暫時用第一次活動報名時間代替註冊時間
            var firstActivity = await _context.UserActivityRegistrations
                .Where(uar => uar.UserId == userId)
                .OrderBy(uar => uar.RegisterTime)
                .FirstOrDefaultAsync();
            
            return firstActivity != null && firstActivity.RegisterTime <= sixMonthsAgo;
        }
    }

    // 成就定義類別
    public class AchievementDefinition
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // activity, purchase, comprehensive
        public int Tier { get; set; } = 1; // 難度等級：1(簡單) 2(中等) 3(困難) 4(專家)
    }

    // 用戶成就視圖模型
    public class UserAchievementViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Tier { get; set; } = 1;
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
        public bool IsNew { get; set; }
    }

    // 成就分類統計
    public class AchievementCategoryStats
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int EarnedCount { get; set; }
        public int NewCount { get; set; }
        public double Progress => TotalCount > 0 ? (double)EarnedCount / TotalCount * 100 : 0;
    }
}