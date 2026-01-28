using System.ComponentModel.DataAnnotations;

namespace NGOPlatformWeb.Models.Entity
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [StringLength(10)]
        public string ?IdentityNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string ?Email { get; set; }

        [StringLength(60)]
        public string ?Password { get; set; }

        [StringLength(20)]
        public string ?Phone { get; set; }

        [StringLength(50)]
        public string ?Name { get; set; }
        
        [StringLength(255)]
        public string ?ProfileImage { get; set; }
    }
}
