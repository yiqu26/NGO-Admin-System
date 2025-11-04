using NGOPlatformWeb.Models.ViewModels.Profile;

namespace NGOPlatformWeb.Models.ViewModels.ActivityRegistrations
{
    public class UserActivityRegistrationsViewModel : BaseActivityRegistrationsViewModel
    {
        public string UserName { get; set; } = string.Empty;
        
        // 實作基類的抽象屬性
        public override string PersonName 
        { 
            get => UserName; 
            set => UserName = value; 
        }
    }
}