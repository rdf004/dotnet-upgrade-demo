using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoCommerce.Data.Entities
{
    [Table("OrderItems")]
    [Serializable]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}
