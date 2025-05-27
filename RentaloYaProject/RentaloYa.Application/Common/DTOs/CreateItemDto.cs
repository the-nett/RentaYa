using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.DTOs
{
    public class CreateItemDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Quantity { get; set; }

        // CAMBIO AQUÍ: Ahora son int para coincidir con la entidad Item y el parsing en el controlador
        [Required(ErrorMessage = "La categoría es requerida.")]
        public int CategoryId { get; set; } // Cambiado a int y renombrado a CategoryId

        // CAMBIO AQUÍ: Ahora son int para coincidir con la entidad Item y el parsing en el controlador
        [Required(ErrorMessage = "El tipo de renta es requerido.")]
        public int RentalTypeId { get; set; } // Cambiado a int y renombrado a RentalTypeId

        [StringLength(255, ErrorMessage = "La ubicación no puede exceder los 255 caracteres.")]
        public string? Location { get; set; }

        public byte[]? ImageData { get; set; }
    }
}