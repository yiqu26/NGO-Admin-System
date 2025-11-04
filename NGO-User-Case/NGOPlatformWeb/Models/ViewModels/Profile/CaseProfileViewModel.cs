using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Profile
{
    public class CaseProfileViewModel : BaseProfileViewModel
    {
        [Display(Name = "生日")]
        public DateTime? Birthday { get; set; }
        
        [Required(ErrorMessage = "請輸入地址")]
        [StringLength(200, ErrorMessage = "地址長度不能超過200個字")]
        [Display(Name = "地址")]
        public string Address { get; set; } = string.Empty;
        
        // 活動報名總覽
        public List<CaseActivitySummary> RecentActivities { get; set; } = new List<CaseActivitySummary>();
        
        // 物資申請總覽（個案專有）
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        // 物資申請的詳細資料將由其他組員實作
    }
}
