using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Repository;
using RentaloYa.Web.ViewModels.Garage;
using RentalWeb.Web.ViewModels.Garage;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RentalWeb.Web.Controllers
{
    [Authorize]
    public class GarageController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Item> _itemRepo;
        private readonly IRepository<RentalType> _rentalTypeRepo;
        private readonly IUserRepository _userRepository;

        public GarageController(IItemService itemService, IRepository<Category> categoryRepo, IRepository<RentalType> rentalTypeRepo, IUserRepository userRepository, IRepository<Item> itemRepository)
        {
            _itemService = itemService;
            _categoryRepo = categoryRepo;
            _rentalTypeRepo = rentalTypeRepo;
            _userRepository = userRepository;
            _itemRepo = itemRepository;
        }
        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email);
            var dtos = await _itemService.GetItemsByUserEmailAsync(correo);

            var viewModel = dtos.Select(dto => new GarageItemViewModel
            {
                Id = dto.Id,
                Nombre = dto.Name,
                Cantidad = 1, // o lo que corresponda
                TipoRenta = dto.RentalTypeName,
                Disponible = dto.Status == "Disponible"
            }).ToList();

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> AddItem()
        {
            var categories = await _categoryRepo.GetAllAsync();
            IEnumerable<SelectListItem> list = categories.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            });
            ViewData["CategoryList"] = list;
            var rentalTypes = await _rentalTypeRepo.GetAllAsync();
            IEnumerable<SelectListItem> list2 = rentalTypes.Select(g => new SelectListItem
            {
                Text = g.TypeName,
                Value = g.Id.ToString()
            });
            ViewData["RentalTypeList"] = list2;
            var viewModel = new CreateItemViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ActionName("AddItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var correo = User.FindFirstValue(ClaimTypes.Email);
                    var userCurrent = await _userRepository.GetUserByEmailAsync(correo); // <-- te faltaba el await

                    var newItem = new Item
                    {
                        OwnerId = userCurrent.Id,
                        Name = item.Name,
                        Description = item.Description,
                        RentalTypeId = int.Parse(item.RentType),
                        Price = item.Price,
                        ImageUrl = item.ImageUrl,
                        CategoryId = int.Parse(item.Category),
                        Location = item.Location,
                        ItemStatusId = 1,
                        QuantityAvailable = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _itemRepo.AddAsync(newItem);

                    return RedirectToAction("Index", "Garage");
                }
            }
            catch
            {
                var categories = await _categoryRepo.GetAllAsync();
                IEnumerable<SelectListItem> list = categories.Select(g => new SelectListItem
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                });
                ViewData["CategoryList"] = list;

                var rentalTypes = await _rentalTypeRepo.GetAllAsync();
                IEnumerable<SelectListItem> list2 = rentalTypes.Select(g => new SelectListItem
                {
                    Text = g.TypeName,
                    Value = g.Id.ToString()
                });
                ViewData["RentalTypeList"] = list2;

                return View("AddItem", item);
            }

            // Si ModelState no es válido, recarga la vista con los select lists
            var cats = await _categoryRepo.GetAllAsync();
            ViewData["CategoryList"] = cats.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            });

            var rents = await _rentalTypeRepo.GetAllAsync();
            ViewData["RentalTypeList"] = rents.Select(g => new SelectListItem
            {
                Text = g.TypeName,
                Value = g.Id.ToString()
            });

            return View("AddItem", item);
        }
    }
}
