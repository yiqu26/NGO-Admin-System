using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(50, ErrorMessage = "姓名長度不能超過50個字")]
        [Display(Name = "姓名")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [Display(Name = "電子信箱")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字之間")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "請確認密碼")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不一致")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入電話號碼")]
        [StringLength(20, ErrorMessage = "電話號碼長度不能超過20個字")]
        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        [Display(Name = "電話號碼")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入身份證字號")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "身份證字號必須為10個字")]
        [Display(Name = "身份證字號")]
        public string IdentityNumber { get; set; } = string.Empty;
    }
}