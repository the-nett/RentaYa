using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class ItemTag
    {
        [Required]
        [Column("item_id")] // Nombre de columna para la base de datos
        public int ItemId { get; set; }

        [Required]
        [Column("tag_id")] // Nombre de columna para la base de datos
        public int TagId { get; set; }

        [Required]
        [Column(TypeName = "real")] // 'real' para float en SQL Server, o 'float' si usas otro DB
        public float Confidence { get; set; }

        // Propiedades de navegación
        public Item Item { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
