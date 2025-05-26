using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RentaloYa.Application.Common.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RentalWeb.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ISupabaseAuthProvider _supabaseAuthProvider; // Inyecta el proveedor de autenticación de Supabase

        private readonly Supabase.Client _supabaseClient;

        public AuthController(IAuthService authService, IUserService userService, IConfiguration configuration, ILogger<AuthController> logger, ISupabaseAuthProvider supabaseAuthProvider, Supabase.Client supabaseClient)
        {
            _authService = authService;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _supabaseAuthProvider = supabaseAuthProvider;
            _supabaseClient = supabaseClient;
        }
        public IActionResult InitiateSupabaseLogin()
        {
            string supabaseUrl = _configuration["Supabase:Url"];
            //string redirectUrl = Url.Action("HandleSupabaseCallback", "Auth", null, Request.Scheme);

            string redirectUrl = Url.Action("HandleSupabaseCallback", "Auth", new { protocol = Request.Scheme });
            //string redirectUrl = $"{Request.Scheme}://{Request.Host}/Auth/Callback";


            if (string.IsNullOrEmpty(supabaseUrl))
            {
                return Problem("La URL de Supabase no está configurada.");
            }

            //string authorizationUrl = $"{supabaseUrl}/auth/v1/authorize?provider=google&redirect_to={redirectUrl}";
            string authorizationUrl = $"{supabaseUrl}/auth/v1/authorize?provider=google&redirect_to=https://localhost:7218/auth/callback/supabase";
            return Redirect(authorizationUrl);
        }

        [HttpGet("auth/callback/supabase")]
        public IActionResult HandleSupabaseCallback()
        {
            return View("HandleSupabaseCallback"); // Retorna la vista intermedia
        }

        [HttpPost]
        public async Task<IActionResult> TokenValidation(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                return Json(new { success = false, message = "No se proporcionó el access_token." });

            var supabaseAuthResult = await _supabaseAuthProvider.GetUserInfoByTokenAsync(accessToken);

            if (!supabaseAuthResult.IsAuthenticated)
                return Json(new { success = false, message = "El token de Supabase no es válido." });

            string email = supabaseAuthResult.Email;
            string supabaseId = supabaseAuthResult.Id;
            // Asumiendo que ISupabaseAuthProvider.GetUserInfoByTokenAsync también te da el nombre
            // Si no, necesitarías modificar tu ISupabaseAuthProvider o asignarle un valor predeterminado/email
            string name = supabaseAuthResult.Name ?? email; // Usa el nombre si está disponible, sino el email

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(supabaseId))
                return Json(new { success = false, message = "No se pudo obtener el email o el ID." });

            var user = await _userService.GetUserByEmailAsync(email);

            // --- PASO CLAVE: ESTABLECER LA COOKIE DE AUTENTICACIÓN DE ASP.NET CORE AQUÍ ---
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, supabaseId), // Identificador único del usuario
                new Claim(ClaimTypes.Name, name), // Nombre del usuario que se mostrará en User.Identity.Name
                new Claim(ClaimTypes.Email, email), // Email del usuario
                new Claim("SupabaseAccessToken", accessToken), // Un claim para el access_token
                new Claim("SupabaseRefreshToken", refreshToken) // Un claim para el refresh_token
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            // --- FIN DEL PASO CLAVE ---


            if (user != null)
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            else
            {
                var redirectUrl = Url.Action("CreateProfile", "Profile", new { email = email, supabaseId = supabaseId, userName = name }); // Pasa también el userName si lo necesitas en CreateProfile
                return Json(new { success = true, redirectUrl = redirectUrl });
            }
        }

        [HttpGet]
        [Authorize] // Asegura que solo usuarios autenticados puedan cerrar sesión
        public async Task<IActionResult> Logout()
        {
            // Primero, cierra la sesión en Supabase para invalidar los tokens allá.
            try
            {
                await _supabaseClient.Auth.SignOut();
                _logger.LogInformation("Sesión de Supabase cerrada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión en Supabase.");
                // Opcional: podrías decidir si quieres lanzar una excepción o simplemente continuar
                // con el cierre de sesión local si el logout de Supabase falla.
            }

            // Luego, cierra la sesión en ASP.NET Core eliminando la cookie de autenticación.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Sesión de ASP.NET Core cerrada exitosamente.");

            // Redirige al usuario a la página de inicio o a la página de login.
            return RedirectToAction("Index", "Home");
        }
    }
}