namespace NGOPlatformWeb.Models.Entity
{
    public class SupplyCategory
    {
        public int SupplyCategoryId { get; set; }

        public string? SupplyCategoryName { get; set; }

        public virtual ICollection<Supply> ?Supplies { get; set; }
    }
}
