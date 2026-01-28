using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Profile
{
    public class UserEditViewModel : BaseProfileViewModel
    {
        // 新密碼（可選）- 留空表示不變更密碼
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須為 6-100 個字符")]
        public new string? NewPassword { get; set; }
        
        // 確認新密碼
        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認密碼不一致")]
        public string? ConfirmNewPassword { get; set; }
        
        // 繼承自 BaseProfileViewModel 的屬性：
        // Name, Email, Phone, IdentityNumber, ProfileImage
        // TotalActivitiesRegistered, ActiveRegistrations
    }
}
