using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Profile
{
    // Profile ViewModel 基類，包含用戶和個案的共同屬性
    public abstract class BaseProfileViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(50, ErrorMessage = "姓名長度不能超過50個字")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [Display(Name = "電子信箱")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入電話號碼")]
        [StringLength(20, ErrorMessage = "電話號碼長度不能超過20個字")]
        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        [Display(Name = "電話號碼")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入身份證字號")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "身份證字號必須為10個字")]
        [Display(Name = "身份證字號")]
        public string IdentityNumber { get; set; } = string.Empty;
        
        [Display(Name = "個人頭像")]
        public string? ProfileImage { get; set; }
        
        // 活動報名共同統計
        public int TotalActivitiesRegistered { get; set; }
        public int ActiveRegistrations { get; set; }
        
        // 密碼編輯專用欄位（不做顯示）
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        public string? NewPassword { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
        public string? ConfirmPassword { get; set; }
    }
    
    // 統一的活動摘要基類
    public class ActivitySummaryBase
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    
    // 個案活動摘要 (繼承基類並新增 Category)
    public class CaseActivitySummary : ActivitySummaryBase
    {
        public string Category { get; set; } = string.Empty;
    }
    
    // 一般用戶活動摘要 (直接使用基類)
    public class ActivitySummary : ActivitySummaryBase
    {
        // 一般用戶的活動摘要與基類相同，無需額外屬性
    }
    
    // 購買摘要 (用於一般用戶)
    public class PurchaseSummary
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
    }
    
    // 活動報名項目 (User 和 Case 共用)
    public class ActivityRegistrationItem
    {
        public int RegistrationId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegisterTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        
        // 計算屬性
        public bool IsUpcoming => StartDate > DateTime.Now;
        public bool IsCompleted => EndDate < DateTime.Now;
        public bool IsActive => Status == "registered";
        public string StatusDisplay => Status == "registered" ? "已報名" : "已取消";
        public string CategoryDisplay => Category switch
        {
            "生活" => "生活技能",
            "心靈" => "心靈成長", 
            "運動" => "運動健康",
            _ => Category
        };
    }
    
    // 活動報名列表基類 (User 和 Case 共用)
    public abstract class BaseActivityRegistrationsViewModel
    {
        public abstract string PersonName { get; set; }
        public int TotalRegistrations { get; set; }
        public int ActiveRegistrations { get; set; }
        public List<ActivityRegistrationItem> Registrations { get; set; } = new List<ActivityRegistrationItem>();
    }
}