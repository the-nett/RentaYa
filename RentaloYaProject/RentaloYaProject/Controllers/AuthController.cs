using Microsoft.AspNetCore.Mvc;

namespace RentalWeb.Web.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
