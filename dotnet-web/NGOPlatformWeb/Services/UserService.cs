using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.Purchase;

namespace NGOPlatformWeb.Services
{
    public class UserService : IUserService
    {
        private readonly NGODbContext _context;

        public UserService(NGODbContext context)
        {
            _context = context;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(int userId)
        {
            var activityRegistrations = await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5)
                .ToListAsync();

            var purchaseOrders = await _context.UserOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return new UserStatsDto
            {
                TotalActivities = await _context.UserActivityRegistrations
                    .Where(r => r.UserId == userId)
                    .CountAsync(),
                ActiveActivities = await _context.UserActivityRegistrations
                    .Where(r => r.UserId == userId && r.Status == "registered")
                    .CountAsync(),
                RecentActivities = activityRegistrations.Select(r => new ActivitySummary
                {
                    ActivityId = r.ActivityId,
                    ActivityName = r.Activity?.ActivityName ?? "未知活動",
                    StartDate = r.Activity?.StartDate ?? DateTime.MinValue,
                    Status = r.Status,
                    ImageUrl = r.Activity?.ImageUrl ?? "/images/activity-default.png"
                }).ToList(),
                TotalPurchases = await _context.UserOrders
                    .Where(o => o.UserId == userId)
                    .CountAsync(),
                TotalPurchaseAmount = await _context.UserOrders
                    .Where(o => o.UserId == userId && o.PaymentStatus == "已付款")
                    .SumAsync(o => o.TotalPrice),
                RecentPurchases = purchaseOrders.Select(o => new PurchaseSummary
                {
                    OrderId = o.UserOrderId,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.PaymentStatus ?? "",
                    OrderNumber = o.OrderNumber ?? ""
                }).ToList()
            };
        }

        public async Task SaveUserAsync(User user)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IList<UserActivityRegistration>> GetUserRegistrationsAsync(int userId)
        {
            return await _context.UserActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();
        }

        public async Task<UserPurchaseRecordsViewModel> GetPurchaseRecordsAsync(int userId)
        {
            var orders = await _context.UserOrders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Supply)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderIds = orders.Select(o => o.UserOrderId).ToList();

            var emergencyPurchases = await _context.EmergencyPurchaseRecords
                .Where(ep => orderIds.Contains(ep.UserOrderId))
                .ToListAsync();

            var emergencyNeedIds = emergencyPurchases.Select(ep => ep.EmergencyNeedId).Distinct().ToList();
            var emergencyNeeds = emergencyNeedIds.Any()
                ? await _context.EmergencySupplyNeeds
                    .Where(en => emergencyNeedIds.Contains(en.EmergencyNeedId))
                    .ToListAsync()
                : new List<EmergencySupplyNeeds>();

            return new UserPurchaseRecordsViewModel
            {
                Orders = orders.Select(o => new OrderRecordViewModel
                {
                    OrderId = o.UserOrderId,
                    OrderNumber = o.OrderNumber ?? "",
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    PaymentStatus = o.PaymentStatus ?? "",
                    PaymentMethod = o.PaymentMethod ?? "",
                    OrderSource = o.OrderSource ?? "",
                    EmergencyNeedId = o.EmergencyNeedId,
                    Items = o.OrderSource == "emergency"
                        ? emergencyPurchases.Where(ep => ep.UserOrderId == o.UserOrderId)
                            .Select(ep =>
                            {
                                var need = emergencyNeeds.FirstOrDefault(en => en.EmergencyNeedId == ep.EmergencyNeedId);
                                var imageUrl = need != null && !string.IsNullOrEmpty(need.ImageUrl)
                                    ? need.ImageUrl
                                    : GetEmergencyImageByName(ep.SupplyName);
                                return new OrderItemViewModel
                                {
                                    SupplyName = ep.SupplyName,
                                    Quantity = ep.Quantity,
                                    UnitPrice = ep.UnitPrice,
                                    TotalPrice = ep.UnitPrice * ep.Quantity,
                                    ImageUrl = imageUrl,
                                    IsEmergency = true,
                                    OrderSource = "emergency"
                                };
                            }).ToList()
                        : o.OrderDetails.Select(od => new OrderItemViewModel
                        {
                            SupplyName = od.Supply?.SupplyName ?? "未知物資",
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice,
                            TotalPrice = od.UnitPrice * od.Quantity,
                            ImageUrl = od.Supply?.ImageUrl ?? "/images/default-supply.png",
                            IsEmergency = false,
                            OrderSource = od.OrderSource ?? ""
                        }).ToList()
                }).ToList()
            };
        }

        private static string GetEmergencyImageByName(string supplyName)
        {
            var name = supplyName?.ToLower() ?? "";

            if (name.Contains("胰島素") || name.Contains("藥") || name.Contains("醫療"))
                return "/images/saline.jpg";
            else if (name.Contains("急救包") || name.Contains("醫療急救"))
                return "/images/bandage.png";
            else if (name.Contains("紙尿褲") || name.Contains("尿布"))
                return "/images/wipes.jpg";
            else if (name.Contains("罐頭") || name.Contains("食物"))
                return "/images/corn.png";
            else if (name.Contains("睡袋") || name.Contains("衣"))
                return "/images/coat.png";
            else
                return "/images/user-default.png";
        }
    }
}
