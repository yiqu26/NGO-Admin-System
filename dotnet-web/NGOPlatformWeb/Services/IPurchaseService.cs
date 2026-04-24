using NGOPlatformWeb.Models.Entity;
using NGOPlatformWeb.Models.ViewModels.Purchase;

namespace NGOPlatformWeb.Services
{
    public class ProcessPaymentResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public UserOrder? Order { get; set; }
        public string OrderNumber { get; set; } = "";
    }

    public interface IPurchaseService
    {
        Task<IList<EmergencySupplyNeeds>> GetActiveEmergencyNeedsAsync();
        Task<IList<Supply>> GetRegularSuppliesAsync();
        Task<EmergencySupplyNeeds?> GetEmergencyNeedByIdAsync(int id);
        Task<Supply?> GetSupplyByIdAsync(int id);
        Task<User?> GetUserByIdAsync(int id);
        Task<Case?> GetCaseByIdAsync(int id);
        Task<ProcessPaymentResult> ProcessPaymentAsync(PaymentViewModel model, string? packageType);
        Task<UserOrder?> GetOrderByNumberAsync(string orderNumber);
        Task<UserOrder?> GetOrderByIdForUserAsync(int orderId, int userId);
        Task UpdateOrderNumberAsync(UserOrder order, string newOrderNumber);
        Task<OrderResultViewModel?> GetEcpaySuccessViewModelAsync(string merchantTradeNo);
    }
}
