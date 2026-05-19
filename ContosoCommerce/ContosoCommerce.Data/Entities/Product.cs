using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations
    .Schema;

namespace ContosoCommerce.Data.Entities
{
    [Table("Products")]
    [Serializable]
    public class Product
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption
                .Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category
        {
            get; set;
        }

        public byte[] ImageData { get; set; }

        public byte[] ThumbnailData
        {
            get; set;
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
