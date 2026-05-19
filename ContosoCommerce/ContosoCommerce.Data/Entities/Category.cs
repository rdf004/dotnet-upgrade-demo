using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoCommerce.Data.Entities
{
    [Table("Categories")]
    [Serializable]
    public class Category
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public virtual Category ParentCategory
        {
            get; set;
        }

        public virtual ICollection<Category>
            SubCategories { get; set; }

        public virtual ICollection<Product>
            Products { get; set; }

        public Category()
        {
            SubCategories =
                new HashSet<Category>();
            Products =
                new HashSet<Product>();
        }
    }
}
