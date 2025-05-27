using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentaloYa.Domain.Entities
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OwnerId { get; set; } // FK hacia Users (int, no Guid)

        public required string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public int RentalTypeId { get; set; }
        public RentalType RentalType { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string? Location { get; set; }

        [Required]
        public int ItemStatusId { get; set; }
        public ItemStatus ItemStatus { get; set; }

        public int QuantityAvailable { get; set; }

        public DateTime CreatedAt { get; set; }

        // Propiedad de navegación al usuario que publicó el artículo
        [ForeignKey("OwnerId")]
        public User Owner { get; set; } = null!;

        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
