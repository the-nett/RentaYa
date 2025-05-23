using System.ComponentModel.DataAnnotations;

namespace RentalWeb.Web.ViewModels.Profile
{
    public class CreateProfileVm
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        public Guid SupabaseId { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [Display(Name = "Nombre de Usuario")]
        public string Username { get; set; } = string.Empty; // Inicialización con cadena vacía

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty; // Inicialización con cadena vacía

        [Display(Name = "Fecha de Nacimiento")]
        public DateOnly Birthdate { get; set; }

        [Required(ErrorMessage = "El género es requerido.")]
        [Display(Name = "Género")]
        public int Gender { get; set; }
    }
}
