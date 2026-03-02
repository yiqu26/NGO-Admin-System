namespace NGOPlatformWeb.Models.ViewModels.Dashboard
{
    /// <summary>
    /// 使用者儀表板主要ViewModel
    /// </summary>
    public class UserDashboardViewModel
    {
        public List<MonthlyDonationData> MonthlyDonations { get; set; } = new List<MonthlyDonationData>();
        public List<CategoryDistributionData> CategoryDistribution { get; set; } = new List<CategoryDistributionData>();
        public DonationTrendData DonationTrends { get; set; } = new DonationTrendData();
        public ImpactStatisticsData ImpactStatistics { get; set; } = new ImpactStatisticsData();
    }

    /// <summary>
    /// 月度捐贈數據
    /// </summary>
    public class MonthlyDonationData
    {
        public string Month { get; set; } = string.Empty;
        public decimal EmergencyAmount { get; set; }
        public decimal PackageAmount { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalAmount => EmergencyAmount + PackageAmount;
    }

    /// <summary>
    /// 物資類別分布數據
    /// </summary>
    public class CategoryDistributionData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 捐贈趨勢數據
    /// </summary>
    public class DonationTrendData
    {
        public decimal AverageDonationAmount { get; set; }
        public decimal LargestDonation { get; set; }
        public string DonationFrequency { get; set; } = string.Empty;
        public double GrowthRate { get; set; }
        public int RecentActivityDays { get; set; }
    }

    /// <summary>
    /// 影響力統計數據
    /// </summary>
    public class ImpactStatisticsData
    {
        public int TotalItemsDonated { get; set; }
        public int EmergencyResponseCount { get; set; }
        public int CategoriesHelped { get; set; }
        public int ConsistencyScore { get; set; }
    }
}