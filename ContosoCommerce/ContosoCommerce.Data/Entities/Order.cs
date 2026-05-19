using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContosoCommerce.Core.Enums;

namespace ContosoCommerce.Data.Entities
{
    [Table("Orders")]
    [Serializable]
    public class Order
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public OrderStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(500)]
        public string ShippingAddress { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<OrderItem> Items
        {
            get; set;
        }

        public Order()
        {
            Items = new HashSet<OrderItem>();
        }
    }
}
