using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
