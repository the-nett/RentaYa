using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;
using RentalWeb.Web.ViewModels.Post;
using System.Security.Claims;

namespace RentalWeb.Web.Controllers
{
    public class MyPostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserRepository _userRepository;

        public MyPostsController(IPostService postService, IUserRepository userRepository)
        {
            _postService = postService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var postDtos = await _postService.GetPostsByUserEmailAsync(email);

            var viewModel = postDtos.Select(p => new PostViewModel
            {
                PostId = p.PostId,
                Title = p.Title,
                CreatedAt = p.CreatedAt,
                ItemName = p.ItemName,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                Location =p.Location,
                Price = p.Price,
                RentalType = p.RentalType,
                QuantityAvailable = p.QuantityAvailable

            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Obtener los ítems del usuario a través del servicio
            var userItems = await _postService.GetUserItemsForPostCreationAsync(email);

            // Crear SelectListItems para el DropDownList
            var availableItems = userItems.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Nombre
            }).ToList();

            var viewModel = new CreatePostWebViewModel
            {
                AvailableItems = availableItems
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostWebViewModel model)
        {
            // Antes de validar el modelo, aseguramos que la lista de items esté disponible si volvemos a la vista
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Obtener los ítems del usuario para rellenar el dropdown si el modelo no es válido
            var userItemsForDropdown = await _postService.GetUserItemsForPostCreationAsync(email);
            model.AvailableItems = userItemsForDropdown.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Nombre
            }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }
            var userId = user.Id;


            var createPostDto = new CreatePostDto
            {
                Title = model.Title,
                Description = model.Description,
                ItemId = model.ItemId,
                UserId = userId
            };

            var result = await _postService.CreatePostAsync(createPostDto);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) // id es el PostId
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Llamada al servicio para obtener los datos del post y los ítems
            var postDetailDto = await _postService.GetPostForEditAsync(id, email);

            if (postDetailDto == null)
            {
                TempData["ErrorMessage"] = "Post no encontrado o no tienes permisos para editarlo.";
                return RedirectToAction(nameof(Index));
            }

            // Mapear PostDetailDto a EditPostWebViewModel
            var viewModel = new EditPostWebViewModel
            {
                PostId = postDetailDto.PostId,
                Title = postDetailDto.Title,
                Description = postDetailDto.Description,
                ItemId = postDetailDto.ItemId,
                CurrentItemName = postDetailDto.CurrentItemName,
                // Mapear la lista de ItemDto a SelectListItem
                AvailableItems = postDetailDto.UserItems.Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Nombre
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPostWebViewModel model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }

            // Volver a cargar los ítems para el dropdown si hay errores de validación
            var userItemsForDropdown = await _postService.GetUserItemsForPostCreationAsync(email);
            model.AvailableItems = userItemsForDropdown.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Nombre
            }).ToList();

            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, y el nombre del item actual se perdió, intenta recuperarlo
                if (string.IsNullOrEmpty(model.CurrentItemName))
                {
                    var selectedItem = userItemsForDropdown.FirstOrDefault(i => i.Id == model.ItemId);
                    if (selectedItem != null)
                    {
                        model.CurrentItemName = selectedItem.Nombre;
                    }
                }
                return View(model);
            }

            var editPostDto = new EditPostDto
            {
                PostId = model.PostId,
                Title = model.Title,
                Description = model.Description,
                ItemId = model.ItemId,
                UserId = user.Id
            };

            var result = await _postService.EditPostAsync(editPostDto);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                // Si el mensaje de error es específico del artículo actual, y el nombre del item actual se perdió, intenta recuperarlo
                if (string.IsNullOrEmpty(model.CurrentItemName) && result.Message.Contains("artículo seleccionado"))
                {
                    var selectedItem = userItemsForDropdown.FirstOrDefault(i => i.Id == model.ItemId);
                    if (selectedItem != null)
                    {
                        model.CurrentItemName = selectedItem.Nombre;
                    }
                }
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id) // 'id' será el PostId
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            // Llamada al servicio para eliminar el post
            var result = await _postService.DeletePostAsync(id, email);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
