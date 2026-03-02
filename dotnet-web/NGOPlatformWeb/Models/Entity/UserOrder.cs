using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    // 使用者訂單主檔 - 儲存捐贈訂單基本資訊
    public class UserOrder
    {
        [Key]
        public int UserOrderId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string? OrderNumber { get; set; } = "";

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string? PaymentStatus { get; set; } = "已付款"; // 跳過付款流程，直接確認為已付款

        [StringLength(50)]
        public string? PaymentMethod { get; set; } = "credit_card"; // 付款方式：credit_card, atm, ecpay

        [StringLength(50)]
        public string? OrderSource { get; set; } = "regular"; // 訂單來源：package, emergency, regular

        public int? EmergencyNeedId { get; set; } // 緊急需求ID

        // 導覽屬性
        public virtual User? User { get; set; }
        public virtual ICollection<UserOrderDetail> OrderDetails { get; set; } = new List<UserOrderDetail>();
    }
}