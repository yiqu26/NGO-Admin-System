using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Profile;
using NGOPlatformWeb.Models.ViewModels.Purchase;

namespace NGOPlatformWeb.Services
{
    public class UserStatsDto
    {
        public int TotalActivities { get; set; }
        public int ActiveActivities { get; set; }
        public List<ActivitySummary> RecentActivities { get; set; } = new();
        public int TotalPurchases { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public List<PurchaseSummary> RecentPurchases { get; set; } = new();
    }

    public interface IUserService
    {
        Task<UserStatsDto> GetUserStatsAsync(int userId);
        Task SaveUserAsync(User user);
        Task<IList<UserActivityRegistration>> GetUserRegistrationsAsync(int userId);
        Task<UserPurchaseRecordsViewModel> GetPurchaseRecordsAsync(int userId);
    }
}
