using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class PublicationTag
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public int TagId { get; set; }

        public float Confidence { get; set; }

        // Navegación
        public Post Post { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
