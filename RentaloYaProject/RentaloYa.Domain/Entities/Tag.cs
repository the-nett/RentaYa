using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public ICollection<PublicationTag> PublicationTags { get; set; } = new List<PublicationTag>();
    }
}
