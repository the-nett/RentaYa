using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Repository;

namespace RentaloYa.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Gender> _genderRepo;
        // Fixed declaration issue

        public UserController(IRepository<User> userRepo, IRepository<Gender> genderRepo)
        {
            _userRepo = userRepo;
            _genderRepo = genderRepo;
        }

        public async Task<IActionResult> AdminUsersPanel()
        {
            var users = await _userRepo.GetAllAsync();
            return View(users);
        }

        public async Task<IActionResult> CreateUser()
        {
            // Await the Task to get the actual IEnumerable<Gender> result
            var genders = await _genderRepo.GetAllAsync();
            IEnumerable<SelectListItem> list = genders.Select(g => new SelectListItem
            {
                Text = g.GenderName,
                Value = g.IdGender.ToString()
            });

            ViewData["GendersList"] = list;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            try
            {
                await _userRepo.AddAsync(user);
                TempData["Message"] = "El usuario fue creado correctamente.";
                TempData["IsSuccess"] = true;
                return RedirectToAction(nameof(AdminUsersPanel));
            }
            catch (Exception)
            {
                TempData["Message"] = "Ocurrió un error al crear el usuario.";
                TempData["IsSuccess"] = false;
                return View(user);
            }
        }


        public async Task<IActionResult> UpdateUser(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userRepo.UpdateAsync(user);

            return View(user);
        }


        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            try
            {
                _userRepo.UpdateAsync(user);
                _userRepo.SaveChangesAsync();
                TempData["Message"] = "El usuario fue actualizado correctamente.";
                TempData["IsSuccess"] = true;
                return RedirectToAction(nameof(AdminUsersPanel));
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Ocurrió un error al actualizar el usuario.";
                TempData["IsSuccess"] = false;
                return View(user);
            }
        }
    }
}