using NGOPlatformWeb.Models.ViewModels.Profile;

namespace NGOPlatformWeb.Models.ViewModels.ActivityRegistrations
{
    public class CaseActivityRegistrationsViewModel : BaseActivityRegistrationsViewModel
    {
        public string CaseName { get; set; } = string.Empty;
        
        // 實作基類的抽象屬性
        public override string PersonName 
        { 
            get => CaseName; 
            set => CaseName = value; 
        }
    }
}