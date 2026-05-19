using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations
    .Schema;

namespace ContosoCommerce.Data.Entities
{
    [Table("AuthTokens")]
    [Serializable]
    public class AuthToken
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption
                .Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(512)]
        public string Token { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
    }
}
