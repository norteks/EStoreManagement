// Models.cs

using System.ComponentModel.DataAnnotations;

namespace EStoreManagement.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        public int Stock { get; set; }
    }

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public IList<Product> Products { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
    }
}