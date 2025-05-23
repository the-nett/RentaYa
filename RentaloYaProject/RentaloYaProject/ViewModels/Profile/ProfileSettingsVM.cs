using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Web.ViewModels.Profile
{
    public class ProfileSettingsVM
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [Display(Name = "Nombre de Usuario")]
        public string Username { get; set; } = string.Empty; // Inicialización con cadena vacía

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty; // Inicialización con cadena vacía

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        public DateOnly Birthdate { get; set; }

        [Required(ErrorMessage = "El género es requerido.")]
        [Display(Name = "Género")]
        public int Gender { get; set; }
    }
}
