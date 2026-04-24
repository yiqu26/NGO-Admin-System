using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels;

namespace NGOPlatformWeb.Services
{
    public interface ICaseService
    {
        Task<IList<Supply>> GetSuppliesAsync(string? category);
        Task ApplySupplyAsync(int caseId, int supplyId, int quantity);
        Task<(CaseLogin? login, Case? cas)> GetCaseByEmailAsync(string email);
        Task<SupplyRecordViewModel> GetCasePurchaseListAsync(int caseId);
        Task<IList<CaseActivityRegistrations>> GetCaseActivityRegistrationsAsync(int caseId);
        Task<(int total, int active, IList<CaseActivityRegistrations> recent)> GetCaseActivityCountsAsync(int caseId);
        Task UpdateCaseProfileAsync(CaseLogin login, Case cas, string passwordHash, string? imagePath);
        Task UpdateCaseImageAsync(Case cas, string imagePath);
    }
}
