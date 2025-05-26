using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;


namespace RentaloYa.Web.ViewModels.Garage
{
    public class CreateItemViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre del artículo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre del Artículo")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El tipo de renta es obligatorio.")]
        [Display(Name = "Tipo de Renta")]
        public string RentType { get; set; } // Para almacenar "Día", "Semana", "Mes"

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Display(Name = "Precio por Unidad")]
        public decimal Price { get; set; }

        [Url(ErrorMessage = "La URL de la imagen no es válida.")]
        [Display(Name = "URL de la Imagen")]
        public string? ImageUrl { get; set; }
        [Display(Name = "Subir Imagen")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public string Category { get; set; }

        [Required(ErrorMessage = "La ubicación es obligatoria.")]
        [StringLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres.")]
        [Display(Name = "Ubicación")]
        public string Location { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        [Display(Name = "Cantidad Disponible")]
        public int Quantity { get; set; }

        // Propiedades para los desplegables (SelectListItems)
        public List<SelectListItem> RentTypes { get; set; }
        public List<SelectListItem> Categories { get; set; }

        public CreateItemViewModel()
        {
            // Inicializamos las listas para evitar errores de referencia nula
            RentTypes = new List<SelectListItem>();
            Categories = new List<SelectListItem>();
        }
    }
}