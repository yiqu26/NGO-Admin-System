using System.Security.Claims;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;
using NGOPlatformWeb.Repositories;

namespace NGOPlatformWeb.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;

        public ActivityService(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<ActivityIndexViewModel> GetActivitiesForRoleAsync(string userRole, string? category = null, string? keyword = null)
        {
            return await GetActivitiesForRoleAsync(userRole, null, category, keyword);
        }

        public async Task<ActivityIndexViewModel> GetActivitiesForRoleAsync(string userRole, int? userId, string? category = null, string? keyword = null)
        {
            // 根據角色決定要查詢的活動類型
            string targetAudience = userRole switch
            {
                "Case" => "case",
                "User" or "Guest" => "public",
                _ => "public"
            };

            var activities = await _activityRepository.GetActivitiesWithFiltersAsync(targetAudience, category, keyword);
            
            // 查詢用戶已報名的活動ID
            var registeredActivityIds = new List<int>();
            if (userId.HasValue && userRole != "Guest")
            {
                registeredActivityIds = await _activityRepository.GetUserRegisteredActivityIdsAsync(userId.Value, userRole);
            }

            return BuildViewModelForRole(activities, userRole, registeredActivityIds, category, keyword);
        }

        public string DetermineUserRole(ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return "Guest";

            var role = user.FindFirstValue(ClaimTypes.Role);
            return role switch
            {
                "User" => "User",
                "Case" => "Case",
                _ => "Guest"
            };
        }

        public async Task<Activity?> GetActivityByIdAsync(int activityId)
        {
            return await _activityRepository.GetActivityByIdAsync(activityId);
        }

        public ActivityIndexViewModel BuildViewModelForRole(List<Activity> activities, string userRole, string? category = null, string? keyword = null)
        {
            return BuildViewModelForRole(activities, userRole, new List<int>(), category, keyword);
        }

        public ActivityIndexViewModel BuildViewModelForRole(List<Activity> activities, string userRole, List<int> registeredActivityIds, string? category = null, string? keyword = null)
        {
            return new ActivityIndexViewModel
            {
                Activities = activities,
                RegisteredActivityIds = registeredActivityIds,
                UserType = userRole,
                IsAuthenticated = userRole != "Guest",
                CurrentAction = userRole switch
                {
                    "Case" => "CaseIndex",
                    "User" => "UserIndex",
                    _ => "PublicIndex"
                },
                Category = category,
                Keyword = keyword
            };
        }

        public async Task<bool> CancelRegistrationAsync(int userId, int activityId, string userType)
        {
            // 驗證參數
            if (userId <= 0 || activityId <= 0 || string.IsNullOrEmpty(userType))
                return false;

            // 驗證用戶類型
            if (userType != "User" && userType != "Case")
                return false;

            // 委託給 Repository 層處理資料庫操作
            return await _activityRepository.CancelUserRegistrationAsync(userId, activityId, userType);
        }

        public async Task<List<int>> GetUserRegisteredActivityIdsAsync(int userId, string userType)
        {
            return await _activityRepository.GetUserRegisteredActivityIdsAsync(userId, userType);
        }

        public async Task<bool> RegisterUserWithCompanionsAsync(int userId, int activityId, int numberOfCompanions)
        {
            // 驗證參數
            if (userId <= 0 || activityId <= 0 || numberOfCompanions < 0)
                return false;

            // 檢查活動是否存在
            var activity = await _activityRepository.GetActivityByIdAsync(activityId);
            if (activity == null)
                return false;

            // 檢查是否已經報名過
            var registeredIds = await _activityRepository.GetUserRegisteredActivityIdsAsync(userId, "User");
            if (registeredIds.Contains(activityId))
                return false;

            // 檢查名額是否足夠 (自己 + 同伴) - 即時計算實際參與人數
            var requiredSlots = 1 + numberOfCompanions;
            var actualCurrentParticipants = await _activityRepository.GetActualParticipantsCountAsync(activityId);
            var availableSlots = activity.MaxParticipants - actualCurrentParticipants;
            if (requiredSlots > availableSlots)
                return false;

            // 委託給 Repository 層處理
            return await _activityRepository.RegisterUserWithCompanionsAsync(userId, activityId, numberOfCompanions);
        }

        public async Task<int> GetActualParticipantsCountAsync(int activityId)
        {
            return await _activityRepository.GetActualParticipantsCountAsync(activityId);
        }

        public async Task<bool> RegisterCaseAsync(int caseId, int activityId)
        {
            // 驗證參數
            if (caseId <= 0 || activityId <= 0)
                return false;

            // 檢查活動是否存在
            var activity = await _activityRepository.GetActivityByIdAsync(activityId);
            if (activity == null)
                return false;

            // 檢查是否已經報名過
            var registeredIds = await _activityRepository.GetUserRegisteredActivityIdsAsync(caseId, "Case");
            if (registeredIds.Contains(activityId))
                return false;

            // 檢查名額是否足夠 (Case 固定 1 人)
            if (activity.CurrentParticipants >= activity.MaxParticipants)
                return false;

            // 委託給 Repository 層處理
            return await _activityRepository.RegisterCaseAsync(caseId, activityId);
        }
    }
}