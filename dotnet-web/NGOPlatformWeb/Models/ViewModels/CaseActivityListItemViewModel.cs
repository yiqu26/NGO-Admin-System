namespace NGOPlatformWeb.Models.ViewModels
{
    public class CaseActivityListItemViewModel
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
