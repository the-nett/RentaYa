using RentaloYa.Domain.Entities;
using System.ComponentModel.DataAnnotations;

public class ItemStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string StatusName { get; set; }

    public string Description { get; set; }

    // Navegación
    public ICollection<Item> Items { get; set; }
}
