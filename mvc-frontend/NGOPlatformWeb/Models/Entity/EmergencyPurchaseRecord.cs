using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    /// <summary>
    /// 緊急物資認購記錄表
    /// 專門用於記錄民眾對緊急物資的認購情況
    /// </summary>
    public class EmergencyPurchaseRecord
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 關聯的用戶訂單ID
        /// </summary>
        public int UserOrderId { get; set; }
        
        /// <summary>
        /// 緊急需求ID
        /// </summary>
        public int EmergencyNeedId { get; set; }
        
        /// <summary>
        /// 物資名稱（從緊急需求中複製，避免關聯查詢）
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SupplyName { get; set; } = string.Empty;
        
        /// <summary>
        /// 認購數量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 單價
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 認購日期
        /// </summary>
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 個案ID（方便查詢）
        /// </summary>
        public int CaseId { get; set; }
        
        /// <summary>
        /// 付款方式
        /// </summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        // 導覽屬性
        public virtual UserOrder? UserOrder { get; set; }
        public virtual EmergencySupplyNeeds? EmergencyNeed { get; set; }
    }
}