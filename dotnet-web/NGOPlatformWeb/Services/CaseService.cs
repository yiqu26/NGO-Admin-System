using Microsoft.EntityFrameworkCore;
using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;

namespace NGOPlatformWeb.Services
{
    public class CaseService : ICaseService
    {
        private readonly NGODbContext _context;

        public CaseService(NGODbContext context)
        {
            _context = context;
        }

        public async Task<IList<Supply>> GetSuppliesAsync(string? category)
        {
            var query = _context.Supplies
                .Include(s => s.SupplyCategory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(s => s.SupplyCategory != null &&
                    s.SupplyCategory.SupplyCategoryName != null &&
                    s.SupplyCategory.SupplyCategoryName.Contains(category));

            return await query.ToListAsync();
        }

        public async Task ApplySupplyAsync(int caseId, int supplyId, int quantity)
        {
            var need = new RegularSupplyNeeds
            {
                CaseId = caseId,
                SupplyId = supplyId,
                Quantity = quantity,
                ApplyDate = DateTime.Now,
                Status = "pending"
            };

            _context.RegularSuppliesNeeds.Add(need);
            await _context.SaveChangesAsync();
        }

        public async Task<(CaseLogin? login, Case? cas)> GetCaseByEmailAsync(string email)
        {
            var login = await _context.CaseLogins.FirstOrDefaultAsync(c => c.Email == email);
            if (login == null) return (null, null);

            var cas = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == login.CaseId);
            return (login, cas);
        }

        public async Task<SupplyRecordViewModel> GetCasePurchaseListAsync(int caseId)
        {
            var caseName = await _context.Cases
                .Where(c => c.CaseId == caseId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "個案";

            var unreceived = await _context.RegularSuppliesNeeds
                .Include(r => r.Supply).ThenInclude(s => s!.SupplyCategory)
                .Where(r => r.CaseId == caseId && r.Status == "pending")
                .Select(r => new SupplyRecordItem
                {
                    Name = r.Supply != null ? r.Supply.SupplyName ?? "" : "",
                    Category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName ?? "" : "",
                    Quantity = r.Quantity,
                    ApplyDate = r.ApplyDate,
                    PickupDate = r.PickupDate,
                    Status = r.Status ?? "",
                    ImageUrl = r.Supply != null ? r.Supply.ImageUrl : null
                })
                .OrderByDescending(r => r.ApplyDate)
                .ToListAsync();

            var receivedRegular = await _context.RegularSuppliesNeeds
                .Include(r => r.Supply).ThenInclude(s => s!.SupplyCategory)
                .Where(r => r.CaseId == caseId && r.Status == "collected")
                .Select(r => new SupplyRecordItem
                {
                    Name = r.Supply != null ? r.Supply.SupplyName ?? "" : "",
                    Category = r.Supply != null && r.Supply.SupplyCategory != null ? r.Supply.SupplyCategory.SupplyCategoryName ?? "" : "",
                    Quantity = r.Quantity,
                    ApplyDate = r.ApplyDate,
                    PickupDate = r.PickupDate,
                    Status = r.Status ?? "",
                    ImageUrl = r.Supply != null ? r.Supply.ImageUrl : null
                })
                .ToListAsync();

            var receivedEmergency = await _context.EmergencySupplyNeeds
                .Where(e => e.CaseId == caseId && e.Status == "Completed")
                .Select(e => new SupplyRecordItem
                {
                    Name = e.SupplyName ?? "",
                    Category = "緊急物資",
                    Quantity = e.Quantity,
                    ApplyDate = e.CreatedDate ?? DateTime.Now,
                    PickupDate = e.UpdatedDate ?? DateTime.Now,
                    Status = "訪談物資",
                    ImageUrl = e.ImageUrl ?? "/images/emergency-default.png"
                })
                .ToListAsync();

            var received = receivedRegular
                .Concat(receivedEmergency)
                .OrderByDescending(s => s.PickupDate)
                .ToList();

            var allSupplies = unreceived
                .Concat(received.Where(s => s.Status != "訪談物資"))
                .ToList();

            var categoryStats = allSupplies
                .GroupBy(s => s.Category)
                .Select(g => new CategoryStat
                {
                    CategoryName = g.Key ?? "未分類",
                    TotalQuantity = g.Sum(x => x.Quantity),
                    ItemCount = g.Count()
                })
                .ToList();

            return new SupplyRecordViewModel
            {
                CaseName = caseName,
                UnreceivedSupplies = unreceived,
                ReceivedSupplies = received,
                CategoryStats = categoryStats
            };
        }

        public async Task<IList<CaseActivityRegistrations>> GetCaseActivityRegistrationsAsync(int caseId)
        {
            return await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == caseId)
                .OrderByDescending(r => r.RegisterTime)
                .ToListAsync();
        }

        public async Task<(int total, int active, IList<CaseActivityRegistrations> recent)> GetCaseActivityCountsAsync(int caseId)
        {
            var total = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == caseId)
                .CountAsync();

            var active = await _context.CaseActivityRegistrations
                .Where(r => r.CaseId == caseId && r.Status == "registered")
                .CountAsync();

            var recent = await _context.CaseActivityRegistrations
                .Include(r => r.Activity)
                .Where(r => r.CaseId == caseId)
                .OrderByDescending(r => r.RegisterTime)
                .Take(5)
                .ToListAsync();

            return (total, active, recent);
        }

        public async Task UpdateCaseProfileAsync(CaseLogin login, Case cas, string passwordHash, string? imagePath)
        {
            login.Password = passwordHash;
            cas.ProfileImage = imagePath;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCaseImageAsync(Case cas, string imagePath)
        {
            cas.ProfileImage = imagePath;
            await _context.SaveChangesAsync();
        }
    }
}
