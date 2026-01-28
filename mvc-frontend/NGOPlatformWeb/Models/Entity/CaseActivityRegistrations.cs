using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    public class CaseActivityRegistrations
    {
        [Key]
        public int RegistrationId { get; set; }
        public int CaseId { get; set; }
        public int ActivityId { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "registered"; // registered, cancelled

        //[NotMapped]
        //public string? ContactName { get; set; }

        //[NotMapped]
        //public string? Email { get; set; }

        //[NotMapped]
        //public string? Phone { get; set; }

        //[NotMapped]
        //public int? ParticipantCount { get; set; }

        public DateTime RegisterTime { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Case Case { get; set; }
        public virtual Activity Activity { get; set; }
    }
}