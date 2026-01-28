using System;
using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AchievementCode { get; set; } = string.Empty;
        
        [Required]
        public DateTime EarnedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual User? User { get; set; }
    }
}