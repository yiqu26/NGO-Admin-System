using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    // 訂單明細表 - 儲存捐贈物資詳細資訊 (組合包分解為單項)
    public class UserOrderDetail
    {
        [Key]
        public int DetailId { get; set; }

        public int UserOrderId { get; set; }

        public int SupplyId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(50)]
        public string? OrderSource { get; set; } = "regular"; // 明細來源：package, emergency, regular

        public int? EmergencyNeedId { get; set; } // 對應的緊急需求ID

        // 關聯屬性 - 訂單和物資的關聯
        public virtual UserOrder? UserOrder { get; set; }
        public virtual Supply? Supply { get; set; }
    }
}