using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGOPlatformWeb.Models.Entity
{
    public class Case
    {
        [Key]
        public int CaseId { get; set; }

        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? Birthday { get; set; }

        // public string? Address { get; set; }    // 舊欄已移除，改用 City + District + DetailAddress
        public int? WorkerId { get; set; }     
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }

        // 0708 新增欄位
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImage { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailAddress { get; set; }

        // 計算屬性 - 完整地址
        [NotMapped]
        public string FullAddress => $"{City} {District} {DetailAddress}".Trim();

        // 導航屬性
        public virtual CaseLogin? CaseLogin { get; set; }
    }
}
