using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly NGODbContext _context;

        public ActivityRepository(NGODbContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> GetActivitiesWithFiltersAsync(string targetAudience, string? category = null, string? keyword = null)
        {
            var query = _context.Activities
                .Where(a => a.TargetAudience != null && a.TargetAudience == targetAudience);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category != null && a.Category == category);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.ActivityName != null && a.ActivityName.Contains(keyword));
            }

            return await query.ToListAsync();
        }

        public async Task<Activity?> GetActivityByIdAsync(int activityId)
        {
            return await _context.Activities.FindAsync(activityId);
        }

        public async Task<bool> ActivityExistsAsync(int activityId)
        {
            return await _context.Activities.AnyAsync(a => a.ActivityId == activityId);
        }

        public async Task<List<Activity>> GetAllActivitiesAsync()
        {
            return await _context.Activities.ToListAsync();
        }

        public async Task<List<int>> GetUserRegisteredActivityIdsAsync(int userId, string userType)
        {
            if (userType == "Case")
            {
                return await _context.CaseActivityRegistrations
                    .Where(r => r.CaseId == userId && r.Status == "registered")
                    .Select(r => r.ActivityId)
                    .ToListAsync();
            }
            else if (userType == "User")
            {
                return await _context.UserActivityRegistrations
                    .Where(r => r.UserId == userId && r.Status == "registered")
                    .Select(r => r.ActivityId)
                    .ToListAsync();
            }
            
            return new List<int>();
        }

        public async Task<bool> CancelUserRegistrationAsync(int userId, int activityId, string userType)
        {
            try
            {
                if (userType == "Case")
                {
                    var registration = await _context.CaseActivityRegistrations
                        .FirstOrDefaultAsync(r => r.CaseId == userId && r.ActivityId == activityId && r.Status == "registered");

                    if (registration != null)
                    {
                        // 更新報名狀態
                        registration.Status = "cancelled";
                        await _context.SaveChangesAsync(); // 先儲存狀態更新
                        
                        // 再更新活動參與人數 (Case 固定 -1)
                        var activity = await _context.Activities.FindAsync(activityId);
                        if (activity != null)
                        {
                            activity.CurrentParticipants = Math.Max(0, activity.CurrentParticipants - 1);
                            await _context.SaveChangesAsync(); // 再儲存活動更新
                        }
                        
                        return true;
                    }
                }
                else if (userType == "User")
                {
                    var registration = await _context.UserActivityRegistrations
                        .FirstOrDefaultAsync(r => r.UserId == userId && r.ActivityId == activityId && r.Status == "registered");

                    if (registration != null)
                    {
                        // 計算要減少的總人數 (自己 + 同伴)
                        var totalParticipants = 1 + (registration.NumberOfCompanions ?? 0);
                        
                        // 更新報名狀態並重設攜帶人數為0
                        registration.Status = "cancelled";
                        registration.NumberOfCompanions = 0; // 重設攜帶人數避免影響其他人報名
                        await _context.SaveChangesAsync(); // 先儲存狀態更新
                        
                        // 再更新活動參與人數 (分開執行避免觸發器衝突)
                        var activity = await _context.Activities.FindAsync(activityId);
                        if (activity != null)
                        {
                            activity.CurrentParticipants = Math.Max(0, activity.CurrentParticipants - totalParticipants);
                            await _context.SaveChangesAsync(); // 再儲存活動更新
                        }
                        
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterUserWithCompanionsAsync(int userId, int activityId, int numberOfCompanions)
        {
            try
            {
                // 檢查是否有已存在的報名記錄（包括已取消的）
                var existingRegistration = await _context.UserActivityRegistrations
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ActivityId == activityId);

                UserActivityRegistration registration;
                
                bool isNewRegistration = false;
                
                if (existingRegistration != null)
                {
                    // 如果有已存在的記錄，檢查是否為取消狀態
                    if (existingRegistration.Status == "cancelled")
                    {
                        isNewRegistration = true; // 重新報名算作新報名
                    }
                    
                    // 更新已存在的記錄
                    registration = existingRegistration;
                    registration.NumberOfCompanions = numberOfCompanions;
                    registration.Status = "registered";
                    registration.RegisterTime = DateTime.Now;
                }
                else
                {
                    // 如果沒有，創建新記錄
                    registration = new UserActivityRegistration
                    {
                        UserId = userId,
                        ActivityId = activityId,
                        NumberOfCompanions = numberOfCompanions,
                        Status = "registered",
                        RegisterTime = DateTime.Now
                    };
                    _context.UserActivityRegistrations.Add(registration);
                    isNewRegistration = true;
                }

                await _context.SaveChangesAsync(); // 先儲存報名記錄

                // 只有新報名或重新報名才需要增加 CurrentParticipants
                if (isNewRegistration)
                {
                    var activity = await _context.Activities.FindAsync(activityId);
                    if (activity != null)
                    {
                        var totalParticipants = 1 + numberOfCompanions;
                        activity.CurrentParticipants += totalParticipants;
                        await _context.SaveChangesAsync(); // 再儲存活動更新
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetActualParticipantsCountAsync(int activityId)
        {
            // 計算 User 的實際參與人數 (包含同伴)
            var userParticipants = await _context.UserActivityRegistrations
                .Where(r => r.ActivityId == activityId && r.Status == "registered")
                .SumAsync(r => 1 + (r.NumberOfCompanions ?? 0));

            // 計算 Case 的實際參與人數
            var caseParticipants = await _context.CaseActivityRegistrations
                .Where(r => r.ActivityId == activityId && r.Status == "registered")
                .CountAsync();

            return userParticipants + caseParticipants;
        }

        public async Task<bool> RegisterCaseAsync(int caseId, int activityId)
        {
            try
            {
                // 檢查是否有已存在的報名記錄（包括已取消的）
                var existingRegistration = await _context.CaseActivityRegistrations
                    .FirstOrDefaultAsync(r => r.CaseId == caseId && r.ActivityId == activityId);

                CaseActivityRegistrations registration;
                
                bool isNewRegistration = false;
                
                if (existingRegistration != null)
                {
                    // 如果有已存在的記錄，檢查是否為取消狀態
                    if (existingRegistration.Status == "cancelled")
                    {
                        isNewRegistration = true; // 重新報名算作新報名
                    }
                    
                    // 更新已存在的記錄
                    registration = existingRegistration;
                    registration.Status = "registered";
                    registration.RegisterTime = DateTime.Now;
                }
                else
                {
                    // 如果沒有，創建新記錄
                    registration = new CaseActivityRegistrations
                    {
                        CaseId = caseId,
                        ActivityId = activityId,
                        Status = "registered",
                        RegisterTime = DateTime.Now
                    };
                    _context.CaseActivityRegistrations.Add(registration);
                    isNewRegistration = true;
                }

                await _context.SaveChangesAsync(); // 先儲存報名記錄

                // 只有新報名或重新報名才需要增加 CurrentParticipants (Case 固定 +1)
                if (isNewRegistration)
                {
                    var activity = await _context.Activities.FindAsync(activityId);
                    if (activity != null)
                    {
                        activity.CurrentParticipants += 1; // Case 固定 +1
                        await _context.SaveChangesAsync(); // 再儲存活動更新
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}