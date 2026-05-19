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
            var today = DateTime.Today;
            var query = _context.Activities
                .Where(a => a.TargetAudience != null && a.TargetAudience == targetAudience
                         && a.Status == "open"
                         && a.EndDate >= today);

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
            // 用 Transaction 確保狀態更新和人數更新是原子操作，避免其中一步失敗造成資料不一致
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (userType == "Case")
                {
                    var registration = await _context.CaseActivityRegistrations
                        .FirstOrDefaultAsync(r => r.CaseId == userId && r.ActivityId == activityId && r.Status == "registered");

                    if (registration != null)
                    {
                        registration.Status = "cancelled";
                        var activity = await _context.Activities.FindAsync(activityId);
                        if (activity != null)
                            activity.CurrentParticipants = Math.Max(0, activity.CurrentParticipants - 1);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }
                else if (userType == "User")
                {
                    var registration = await _context.UserActivityRegistrations
                        .FirstOrDefaultAsync(r => r.UserId == userId && r.ActivityId == activityId && r.Status == "registered");

                    if (registration != null)
                    {
                        var totalParticipants = 1 + (registration.NumberOfCompanions ?? 0);
                        registration.Status = "cancelled";
                        registration.NumberOfCompanions = 0;
                        var activity = await _context.Activities.FindAsync(activityId);
                        if (activity != null)
                            activity.CurrentParticipants = Math.Max(0, activity.CurrentParticipants - totalParticipants);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RegisterUserWithCompanionsAsync(int userId, int activityId, int numberOfCompanions)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRegistration = await _context.UserActivityRegistrations
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ActivityId == activityId);

                bool isNewRegistration = false;

                if (existingRegistration != null)
                {
                    if (existingRegistration.Status == "cancelled")
                        isNewRegistration = true;

                    existingRegistration.NumberOfCompanions = numberOfCompanions;
                    existingRegistration.Status = "registered";
                    existingRegistration.RegisterTime = DateTime.Now;
                }
                else
                {
                    _context.UserActivityRegistrations.Add(new UserActivityRegistration
                    {
                        UserId = userId,
                        ActivityId = activityId,
                        NumberOfCompanions = numberOfCompanions,
                        Status = "registered",
                        RegisterTime = DateTime.Now
                    });
                    isNewRegistration = true;
                }

                if (isNewRegistration)
                {
                    var activity = await _context.Activities.FindAsync(activityId);
                    if (activity != null)
                    {
                        activity.CurrentParticipants += 1 + numberOfCompanions;
                        // 報名後檢查是否額滿，取代 DB Trigger tr_CheckFullOnRegistration
                        if (activity.CurrentParticipants >= activity.MaxParticipants && activity.Status == "open")
                            activity.Status = "full";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
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
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRegistration = await _context.CaseActivityRegistrations
                    .FirstOrDefaultAsync(r => r.CaseId == caseId && r.ActivityId == activityId);

                bool isNewRegistration = false;

                if (existingRegistration != null)
                {
                    if (existingRegistration.Status == "cancelled")
                        isNewRegistration = true;

                    existingRegistration.Status = "registered";
                    existingRegistration.RegisterTime = DateTime.Now;
                }
                else
                {
                    _context.CaseActivityRegistrations.Add(new CaseActivityRegistrations
                    {
                        CaseId = caseId,
                        ActivityId = activityId,
                        Status = "registered",
                        RegisterTime = DateTime.Now
                    });
                    isNewRegistration = true;
                }

                if (isNewRegistration)
                {
                    var activity = await _context.Activities.FindAsync(activityId);
                    if (activity != null)
                    {
                        activity.CurrentParticipants += 1;
                        // 報名後檢查是否額滿，取代 DB Trigger tr_CheckFullOnRegistration
                        if (activity.CurrentParticipants >= activity.MaxParticipants && activity.Status == "open")
                            activity.Status = "full";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}