using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations
    .Schema;
using ContosoCommerce.Core.Enums;

namespace ContosoCommerce.Data.Entities
{
    [Table("Users")]
    [Serializable]
    public class User
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption
                .Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format(
                    "{0} {1}",
                    FirstName, LastName);
            }
        }
    }
}
