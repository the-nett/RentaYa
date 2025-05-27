using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RentalWeb.Web.ViewModels.Post
{
    public class EditPostWebViewModel
    {
        public int PostId { get; set; } // Necesario para identificar el post a editar

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un artículo.")]
        [Display(Name = "Artículo")]
        public int ItemId { get; set; } // El ID del artículo al que se asociará este post

        // Para mostrar el nombre del artículo actual (solo informativo)
        public string? CurrentItemName { get; set; }

        // Propiedad para la lista de artículos disponibles para el dropdown
        public IEnumerable<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();
    }
}
