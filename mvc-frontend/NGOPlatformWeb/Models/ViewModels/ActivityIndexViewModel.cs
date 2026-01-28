using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class ActivityIndexViewModel
    {
        public List<Activity> Activities { get; set; } = new();
        public List<int> RegisteredActivityIds { get; set; } = new(); // 用戶已報名的活動ID
        public string UserType { get; set; } = "Guest";
        public bool IsAuthenticated { get; set; } = false;
        public string CurrentAction { get; set; } = "PublicIndex";
        public string? Category { get; set; }
        public string? Keyword { get; set; }
    }
}