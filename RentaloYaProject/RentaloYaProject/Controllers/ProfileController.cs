using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;
using RentaloYa.Infrastructure.Repository;
using RentaloYa.Web.ViewModels.Profile;
using RentalWeb.Web.ViewModels.Profile;
using Supabase.Gotrue;

namespace RentaloYa.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Domain.Entities.User> _userRepo;
        private readonly IRepository<Gender> _genderRepo;
        private readonly IUserRepository _userRepository;
        public ProfileController(IRepository<Domain.Entities.User> userRepo, ApplicationDbContext context, IRepository<Gender> genderRepo, IUserRepository userRepoo)
        {
            _context = context;
            _genderRepo = genderRepo;
            _userRepo = userRepo;
            _userRepository = userRepoo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> ProfileSettings(int id)
        {
            Domain.Entities.User? user = _context.Users.FirstOrDefault(x => x.Id == id);
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

        [HttpGet("Profile/Create")]
        [ActionName("CreateProfile")]
        public async Task<IActionResult> ShowCompleteCreateProfileForm(string email, string supabaseId)
        {
            var viewModel = new ProfileSettingsVM
            {
                Email = email, // Pre-carga el email (será readonly en la vista)
                Birthdate = DateOnly.FromDateTime(DateTime.Now.Date) 
            };

            ViewData["SupabaseId"] = supabaseId; // Pasamos el SupabaseId a la vista

            var genders = await _genderRepo.GetAllAsync();
            ViewData["GendersList"] = genders.Select(g => new SelectListItem
            {
                Text = g.GenderName,
                Value = g.IdGender.ToString()
            });

            return View("CreateProfile", viewModel);
        }

        [HttpPost("Profile/Create")]
        [ActionName("CreateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCompleteCreateProfileForm(ProfileSettingsVM model, Guid supabaseId)
        {
            if (model.Birthdate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("Birthdate", "La fecha de nacimiento no puede ser posterior al día actual.");
            }
            if (ModelState.IsValid)
            {
                // Map CreateProfileVm to User entity
                var newUser = new Domain.Entities.User
                {
                    Email = model.Email,
                    IdSupa = supabaseId,
                    Username = model.Username,
                    FullName = model.FullName,
                    Birthdate = model.Birthdate,
                    Gender_Id = model.Gender,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                bool existe = await _userRepository.GetUserByUserNameAsync(newUser.Username);
                if (!existe)
                {
                    await _userRepo.AddAsync(newUser);
                    // Redirigir al usuario a su perfil o a la página principal
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Username", "El nombre de usuario ya existe.");
                }
            }

            // Si llegamos aquí es porque ModelState no es válido o el usuario ya existe
            var genders = await _genderRepo.GetAllAsync();
            ViewData["GendersList"] = genders.Select(g => new SelectListItem
            {
                Text = g.GenderName,
                Value = g.IdGender.ToString()
            });
            ViewData["SupabaseId"] = supabaseId; // Re-pasamos el SupabaseId a la vista

            return View("CreateProfile", model);
        }
    }
}
