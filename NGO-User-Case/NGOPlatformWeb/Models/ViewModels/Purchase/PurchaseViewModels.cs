using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Purchase
{
    /// <summary>
    /// 物資認購的基本ViewModel
    /// 用於顯示商品資訊和數量選擇
    /// </summary>
    public class PurchaseViewModel
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public decimal SupplyPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice => SupplyPrice * Quantity;
        public string? SupplyType { get; set; }
        public int? EmergencyNeedId { get; set; }
        public int? CaseId { get; set; }

        // 購買者資訊（未登入用戶填寫）
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
    }

    /// <summary>
    /// 付款頁面的ViewModel
    /// 包含商品資訊、捐贈者資料、付款方式等
    /// 支援一般認購、緊急需求、組合包等不同類型
    /// </summary>
    public class PaymentViewModel
    {
        public int? SupplyId { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SupplyType { get; set; }
        public int? EmergencyNeedId { get; set; }
        public int? CaseId { get; set; }
        public string? CaseDescription { get; set; } // 個案需求描述
        public int? MaxQuantity { get; set; } // 緊急需求的剩餘數量上限

        // 捐贈者資訊
        [Required(ErrorMessage = "請輸入姓名")]
        public string DonorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        public string DonorEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入手機號碼")]
        [Phone(ErrorMessage = "請輸入有效的手機號碼")]
        public string DonorPhone { get; set; } = string.Empty;

        // 模擬付款資訊
        [Required(ErrorMessage = "請選擇付款方式")]
        public string PaymentMethod { get; set; } = "credit_card";

        public bool IsLoggedIn { get; set; }
        public int? UserId { get; set; }
    }

    /// <summary>
    /// 捐贈完成頁面的ViewModel
    /// 顯示訂單結果、捐贈成功訊息等
    /// </summary>
    public class OrderResultViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsEmergency { get; set; }
        public int? CaseId { get; set; }
    }
}
