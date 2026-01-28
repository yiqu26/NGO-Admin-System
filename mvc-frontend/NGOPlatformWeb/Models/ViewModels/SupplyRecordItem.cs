namespace NGOPlatformWeb.Models.ViewModels
{
    public class SupplyRecordItem
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public DateTime ApplyDate { get; set; } // 若改 nullable 則為 DateTime?
        public DateTime? PickupDate { get; set; }
        public string Status { get; set; } = string.Empty;

    }

}
