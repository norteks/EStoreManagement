using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EStoreManagement.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; }

        public void SetPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                PasswordHash = Convert.ToBase64String(hmac.Key);
                // Use the hash function on the password
            }
        }

        public bool VerifyPassword(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var computedHash = Convert.FromBase64String(PasswordHash);
                // Check if the computed hash matches the password
            }
            return false;
        }
    }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}