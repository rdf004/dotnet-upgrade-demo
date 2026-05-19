using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoCommerce.Data.Entities
{
    [Table("AuditLogs")]
    [Serializable]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; }

        public int EntityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(2000)]
        public string Details { get; set; }

        [MaxLength(256)]
        public string PerformedBy { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
