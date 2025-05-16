using System.ComponentModel.DataAnnotations;

namespace RentalWeb.Web.ViewModels.Profile
{
    public class LoginModuleVM
    {
        public LoginViewModel LoginViewModel { get; set; }
        public RegisterModelVM RegisterViewModel { get; set; }
    }
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce un correo electrónico válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class RegisterModelVM
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce un correo electrónico válido.")]
        public string RegisterEmail { get; set; }

        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        [DataType(DataType.Password)]
        public string RegisterPassword { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("RegisterPassword", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string RegisterConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }
    }
}
