using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RentalWeb.Web.ViewModels.Post
{
    public class CreatePostWebViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un artículo.")]
        [Display(Name = "Artículo")]
        public int ItemId { get; set; } // El ID del artículo seleccionado

        // Nueva propiedad para la lista de artículos disponibles
        public IEnumerable<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();
    }
}
