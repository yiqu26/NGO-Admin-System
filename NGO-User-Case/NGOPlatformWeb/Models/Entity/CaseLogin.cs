using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace NGOPlatformWeb.Models.Entity
{
    [Table("CaseLogins")]
    public class CaseLogin
    {
        [Key, ForeignKey("Case")]
        public int CaseId { get; set; }

        [Required, EmailAddress]
        public string ?Email { get; set; }

        [Required]
        [StringLength(60)]
        public string ?Password { get; set; }

        public DateTime? LastLogin { get; set; }

        // 導航屬性
        public virtual Case ?Case { get; set; }
    }
}
