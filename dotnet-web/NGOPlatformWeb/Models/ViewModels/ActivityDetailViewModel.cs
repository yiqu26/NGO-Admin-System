using NGOPlatformWeb.Models.Entity;

namespace NGOPlatformWeb.Models.ViewModels
{
    public class ActivityDetailViewModel
    {
        public Activity Activity { get; set; } = null!;
        public bool IsRegistered { get; set; }
        public string UserType { get; set; } = "Guest";
        public bool IsAuthenticated { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsFull => AvailableSlots <= 0;
        public bool IsOpen => Activity.Status == "open";
        public bool CanSignup => IsOpen && !IsFull && !IsRegistered;
    }
}
