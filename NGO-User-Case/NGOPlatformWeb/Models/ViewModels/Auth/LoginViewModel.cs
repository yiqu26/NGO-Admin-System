using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.ViewModels.Auth
{
    /// <summary>
    /// 登入頁面的ViewModel
    /// 包含Email和Password，用於一般民眾和個案的登入
    /// 這個不繼承基底類別，因為登入邏輯與個人資料不同
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入電子信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
        [Display(Name = "電子信箱")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string? Password { get; set; }
    }
}

