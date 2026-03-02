using System.Security.Claims;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;

namespace NGOPlatformWeb.Services
{
    public interface IActivityService
    {
        /// <summary>
        /// 根據用戶角色獲取活動清單並建立 ViewModel
        /// </summary>
        Task<ActivityIndexViewModel> GetActivitiesForRoleAsync(string userRole, string? category = null, string? keyword = null);
        
        /// <summary>
        /// 根據用戶角色和ID獲取活動清單並建立 ViewModel（包含報名狀態）
        /// </summary>
        Task<ActivityIndexViewModel> GetActivitiesForRoleAsync(string userRole, int? userId, string? category = null, string? keyword = null);
        
        /// <summary>
        /// 從 ClaimsPrincipal 判斷用戶角色
        /// </summary>
        string DetermineUserRole(ClaimsPrincipal user);
        
        /// <summary>
        /// 獲取單一活動資訊
        /// </summary>
        Task<Activity?> GetActivityByIdAsync(int activityId);
        
        /// <summary>
        /// 根據角色建立對應的 ViewModel
        /// </summary>
        ActivityIndexViewModel BuildViewModelForRole(List<Activity> activities, string userRole, string? category = null, string? keyword = null);
        
        /// <summary>
        /// 取消用戶報名
        /// </summary>
        Task<bool> CancelRegistrationAsync(int userId, int activityId, string userType);
        
        /// <summary>
        /// 取得用戶已報名的活動ID清單
        /// </summary>
        Task<List<int>> GetUserRegisteredActivityIdsAsync(int userId, string userType);
        
        /// <summary>
        /// User 報名 (支援同伴人數)
        /// </summary>
        Task<bool> RegisterUserWithCompanionsAsync(int userId, int activityId, int numberOfCompanions);
        
        /// <summary>
        /// 即時計算活動的實際參與人數
        /// </summary>
        Task<int> GetActualParticipantsCountAsync(int activityId);
        
        /// <summary>
        /// Case 報名 (固定1人)
        /// </summary>
        Task<bool> RegisterCaseAsync(int caseId, int activityId);
    }
}