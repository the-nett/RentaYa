using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
  
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }


        // Navegación
        public ICollection<Item> Items { get; set; }
    }


}
