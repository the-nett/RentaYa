using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;
using RentaloYa.Infrastructure.Repository;
using RentaloYa.Web.ViewModels.Profile;

namespace RentaloYa.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Gender> _genderRepo;
        public ProfileController(ApplicationDbContext context, IRepository<Gender> genderRepo)
        {
            _context = context;
            _genderRepo = genderRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> ProfileSettings(int id)
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
            var genders = await _genderRepo.GetAllAsync();
            IEnumerable<SelectListItem> list = genders.Select(g => new SelectListItem
            {
                Text = g.GenderName,
                Value = g.IdGender.ToString()
            });
            ViewData["GendersList"] = list;

            return View(VMProfileSettings);
        }

    }
}
