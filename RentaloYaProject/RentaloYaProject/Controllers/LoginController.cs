using Microsoft.AspNetCore.Mvc;
using RentalWeb.Web.ViewModels.Profile;
using Supabase.Interfaces;

namespace RentaloYa.Web.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            var model = new LoginModuleVM
            {
                LoginViewModel = new LoginViewModel(),
                RegisterViewModel = new RegisterModelVM()
            };
            return View(model);
            //await _supabaseClient.Auth.SignOut();
        }
    }
}
