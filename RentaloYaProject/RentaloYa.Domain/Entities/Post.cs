using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Navegación
        public Item Item { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
