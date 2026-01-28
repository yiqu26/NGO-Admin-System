using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Repositories
{
    public interface IActivityRepository
    {
        /// <summary>
        /// 根據目標受眾獲取活動清單，可選擇性加入分類和關鍵字篩選
        /// </summary>
        Task<List<Activity>> GetActivitiesWithFiltersAsync(string targetAudience, string? category = null, string? keyword = null);
        
        /// <summary>
        /// 根據 ID 獲取單一活動
        /// </summary>
        Task<Activity?> GetActivityByIdAsync(int activityId);
        
        /// <summary>
        /// 檢查活動是否存在
        /// </summary>
        Task<bool> ActivityExistsAsync(int activityId);
        
        /// <summary>
        /// 獲取所有活動（管理用途）
        /// </summary>
        Task<List<Activity>> GetAllActivitiesAsync();
        
        /// <summary>
        /// 獲取用戶已報名的活動 ID 清單
        /// </summary>
        Task<List<int>> GetUserRegisteredActivityIdsAsync(int userId, string userType);
        
        /// <summary>
        /// 取消用戶報名
        /// </summary>
        Task<bool> CancelUserRegistrationAsync(int userId, int activityId, string userType);
        
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