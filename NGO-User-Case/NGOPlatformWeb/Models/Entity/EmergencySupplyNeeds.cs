using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    /// <summary>
    /// 緊急物資需求表 - 新版設計，不關聯物資總表
    /// 所有操作都在此表內完成
    /// </summary>
    public class EmergencySupplyNeeds
    {
        [Key]
        public int EmergencyNeedId { get; set; }
        
        public int CaseId { get; set; }
        
        public int WorkerId { get; set; }
        
        [StringLength(100)]
        public string? SupplyName { get; set; }
        
        public int Quantity { get; set; }
        
        public int CollectedQuantity { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string? Priority { get; set; }
        
        [StringLength(20)]
        public string? Status { get; set; }
        
        public DateTime? CreatedDate { get; set; }
        
        public DateTime? UpdatedDate { get; set; }
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// 計算剩餘需求量
        /// </summary>
        public int RemainingQuantity => Math.Max(0, Quantity - CollectedQuantity);
        
        /// <summary>
        /// 檢查是否已滿足需求
        /// </summary>
        public bool IsFulfilled => CollectedQuantity >= Quantity;
    }
}