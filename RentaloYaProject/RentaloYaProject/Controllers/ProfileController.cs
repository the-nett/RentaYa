using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;
using RentaloYa.Web.ViewModels.Profile;

namespace RentaloYa.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ProfileSettings(int id)
        {
            User? user = _context.Users.FirstOrDefault(x => x.Id == id);
            //if (user == null)
            //{
            //    return NotFound();
            //}
            var VMProfileSettings = new ProfileSettingsVM
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Birthdate = user.Birthdate,
                Gender = user.Gender_Id
            };
            IEnumerable<SelectListItem> list = _context.Genders.ToList().Select(u => new SelectListItem
            {
                Text = u.GenderName,
                Value = u.IdGender.ToString()
            });
            ViewData["GendersList"] = list;

            return View(VMProfileSettings);
        }

    }
}
