using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class RentalType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TypeName { get; set; }


        // Navegación
        public ICollection<Item> Items { get; set; }
    }
}
