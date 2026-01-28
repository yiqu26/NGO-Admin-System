using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Models
{
    /// <summary>
    /// 首頁的ViewModel
    /// 包含最新活動、影響力統計、使命陳述等首頁元素
    /// </summary>
    public class HomeViewModel
    {
        public List<CarouselItem> CarouselItems { get; set; } = new List<CarouselItem>();
        public List<NGOPlatformWeb.Models.Entity.Activity> RecentActivities { get; set; } = new List<NGOPlatformWeb.Models.Entity.Activity>();
        public ImpactStats ImpactStats { get; set; } = new ImpactStats();
        public MissionStatement MissionStatement { get; set; } = new MissionStatement();
    }

    /// <summary>
    /// 首頁輪播圖的項目
    /// </summary>
    public class CarouselItem
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public string ButtonLink { get; set; } = string.Empty;
    }


    /// <summary>
    /// 影響力統計
    /// </summary>
    public class ImpactStats
    {
        public int TotalActivities { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalCases { get; set; }
        public int TotalSupplies { get; set; }
    }
    
    /// <summary>
    /// 使命陳述
    /// </summary>
    public class MissionStatement
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
