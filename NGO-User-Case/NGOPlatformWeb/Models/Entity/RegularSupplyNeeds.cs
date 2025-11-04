using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class RegularSupplyNeeds
    {
        [ Key ]
        public int RegularNeedId { get; set; }

        public int CaseId { get; set; }  // ✅ ← 必須有這個欄位才能用 r.CaseId 篩選

        public int SupplyId { get; set; }

        public int Quantity { get; set; }

        public DateTime ApplyDate { get; set; }

        public DateTime? PickupDate { get; set; }

        public string ?Status { get; set; }

        public virtual Supply ?Supply { get; set; }  // ✅ 導覽屬性
    }
}
