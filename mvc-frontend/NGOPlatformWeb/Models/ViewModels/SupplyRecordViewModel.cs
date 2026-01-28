using System.Collections.Generic;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class SupplyRecordViewModel
    {
        public List<SupplyRecordItem>? UnreceivedSupplies { get; set; }
        public List<SupplyRecordItem>? ReceivedSupplies { get; set; }
        public List<SupplyRecordItem>? EmergencySupplies { get; set; }

        // ✅ 新增：右邊統計用
        public List<CategoryStat>? CategoryStats { get; set; }
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }      // 總數量
        public int ItemCount { get; set; }          // 幾種物資
    }
}