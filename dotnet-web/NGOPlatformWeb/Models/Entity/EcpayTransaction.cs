using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    public class EcpayTransaction
    {
        [Key]
        public int EcpayTransactionId { get; set; }

        public int UserOrderId { get; set; }

        [StringLength(50)]
        public string? EcpayTradeNo { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public string? ResponseData { get; set; }

        // 導覽屬性
        public virtual UserOrder? UserOrder { get; set; }
    }
}