using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Repository;
using RentaloYa.Web.ViewModels.Garage;
using RentalWeb.Web.ViewModels.Garage;
using Supabase.Interfaces;
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
        private readonly IConfiguration _configuration;
        private readonly Supabase.Client _supabaseClient;

        public GarageController(IItemService itemService, IRepository<Category> categoryRepo, IRepository<RentalType> rentalTypeRepo, IUserRepository userRepository, IRepository<Item> itemRepository, IConfiguration configuration, Supabase.Client supabaseClient)
        {
            _itemService = itemService;
            _categoryRepo = categoryRepo;
            _rentalTypeRepo = rentalTypeRepo;
            _userRepository = userRepository;
            _itemRepo = itemRepository;
            _configuration = configuration;
            _supabaseClient = supabaseClient;
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
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // --- Lógica para subir imagen a Supabase Storage ---
                    string? imageUrl = null;
                    if (model.ImageFile != null)
                    {
                        imageUrl = await UploadImageToSupabase(model.ImageFile);
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            ModelState.AddModelError("ImageFile", "Error al subir la imagen a Supabase.");
                            await PopulateDropdowns();
                            return View("AddItem", model);
                        }
                    }
                    model.ImageUrl = imageUrl; // Asignar la URL obtenida al ViewModel

                    // --- Fin lógica subida imagen ---

                    var correo = User.FindFirstValue(ClaimTypes.Email);
                    var userCurrent = await _userRepository.GetUserByEmailAsync(correo);

                    var newItem = new Item
                    {
                        OwnerId = userCurrent.Id,
                        Name = model.Name,
                        Description = model.Description,
                        RentalTypeId = int.Parse(model.RentType),
                        Price = model.Price,
                        ImageUrl = model.ImageUrl, // Se guarda la URL de Supabase Storage
                        CategoryId = int.Parse(model.Category),
                        Location = model.Location,
                        ItemStatusId = 1,
                        QuantityAvailable = model.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _itemRepo.AddAsync(newItem);

                    TempData["SuccessMessage"] = "Artículo añadido exitosamente.";
                    return RedirectToAction("Index", "Garage");
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error al añadir el artículo.");
                ModelState.AddModelError("", "Ocurrió un error al intentar añadir el artículo. Por favor, inténtelo de nuevo.");
            }

            await PopulateDropdowns();
            return View("AddItem", model);
        }

        // Método GET para la vista de edición
        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {

            var itemToEdit = await _itemRepo.GetByIdAsync(id);

            if (itemToEdit == null)
            {
                // Manejar caso donde el item no se encuentra
                TempData["ErrorMessage"] = "El artículo no fue encontrado.";
                return RedirectToAction("Index");
            }

            // 2. Mapear la entidad Item a tu CreateItemViewModel
            // Dado que CreateItemViewModel ya tiene 'Id', podemos reutilizarlo.
            var viewModel = new CreateItemViewModel
            {
                Id = itemToEdit.Id, // MUY IMPORTANTE: Pasar el Id al ViewModel
                Name = itemToEdit.Name,
                Description = itemToEdit.Description,
                Price = itemToEdit.Price,
                ImageUrl = itemToEdit.ImageUrl,
                Location = itemToEdit.Location,
                Quantity = itemToEdit.QuantityAvailable,
                // Convertimos los IDs de las entidades a string para que coincidan con las propiedades del ViewModel
                Category = itemToEdit.CategoryId.ToString(),
                RentType = itemToEdit.RentalTypeId.ToString()
                // Si el estado es editable, también lo mapearías aquí
            };

            // 3. Obtener listas para los dropdowns
            await PopulateDropdowns(); // Este método pobla ViewData

            return View(viewModel);
        }
        //-----------------Editar item----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(CreateItemViewModel model) // Reutilizamos CreateItemViewModel
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            try
            {
                var itemToUpdate = await _itemRepo.GetByIdAsync(model.Id);

                if (itemToUpdate == null)
                {
                    TempData["ErrorMessage"] = "El artículo a actualizar no fue encontrado.";
                    return RedirectToAction("Index");
                }

                // --- Lógica para subir/actualizar imagen a Supabase Storage ---
                if (model.ImageFile != null)
                {
                    // Si hay una nueva imagen, subirla y obtener la nueva URL
                    string? newImageUrl = await UploadImageToSupabase(model.ImageFile);
                    if (string.IsNullOrEmpty(newImageUrl))
                    {
                        ModelState.AddModelError("ImageFile", "Error al subir la nueva imagen a Supabase.");
                        await PopulateDropdowns();
                        return View(model);
                    }

                    // Opcional: Eliminar la imagen anterior de Supabase Storage
                    // Esto requiere que la URL anterior se limpie correctamente.
                    string oldImageUrl = itemToUpdate.ImageUrl;
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        await DeleteImageFromSupabase(oldImageUrl);
                    }

                    itemToUpdate.ImageUrl = newImageUrl; // Asignar la nueva URL
                }
                // Si model.ImageFile es null, la ImageUrl existente se mantiene si no se ha cambiado

                // --- Fin lógica subida/actualización imagen ---

                itemToUpdate.Name = model.Name;
                itemToUpdate.Description = model.Description;
                itemToUpdate.Price = model.Price;
                // itemToUpdate.ImageUrl = model.ImageUrl; // Esta línea se mueve arriba si se sube nueva imagen
                itemToUpdate.Location = model.Location;
                itemToUpdate.QuantityAvailable = model.Quantity;
                itemToUpdate.CategoryId = int.Parse(model.Category);
                itemToUpdate.RentalTypeId = int.Parse(model.RentType);

                await _itemRepo.UpdateAsync(itemToUpdate);

                TempData["SuccessMessage"] = "Artículo actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error al actualizar el artículo con ID {ItemId}", model.Id);
                ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar el artículo. Por favor, inténtelo de nuevo.");
                await PopulateDropdowns();
                return View(model);
            }
        }

        /// <summary>
        /// Método auxiliar para poblar los dropdowns de categorías y tipos de renta.
        /// Este método es reutilizado por AddItem y EditItem.
        /// </summary>
        private async Task PopulateDropdowns()
        {
            var categories = await _categoryRepo.GetAllAsync();
            ViewData["CategoryList"] = categories.Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            }).ToList(); // .ToList() es una buena práctica aquí

            var rentalTypes = await _rentalTypeRepo.GetAllAsync();
            ViewData["RentalTypeList"] = rentalTypes.Select(g => new SelectListItem
            {
                Text = g.TypeName,
                Value = g.Id.ToString()
            }).ToList(); // .ToList() es una buena práctica aquí
        }

        [HttpPost] // Es más seguro usar HttpPost para operaciones de eliminación
        [ValidateAntiForgeryToken] // Siempre usar para POSTs que modifican datos
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                await _itemRepo.DeleteAsync(id);

                TempData["SuccessMessage"] = "Artículo eliminado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Aquí deberías loguear la excepción para depuración
                // _logger.LogError(ex, "Error al eliminar el artículo con ID {ItemId}", id);

                TempData["ErrorMessage"] = "Ocurrió un error al intentar eliminar el artículo.";
                return RedirectToAction("Index"); // O a una página de error
            }
        }

        //SUpabase Storage---------------------------------------------------------
        private async Task<string?> UploadImageToSupabase(IFormFile imageFile)
        {
            var bucketName = _configuration["Supabase:BucketName"]; // Obtener el nombre del bucket de la configuración
            if (string.IsNullOrEmpty(bucketName))
            {
                // Loguear error: el nombre del bucket no está configurado
                return null;
            }

            // Generar un nombre de archivo único para evitar colisiones
            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(imageFile.FileName)}";
            var filePath = $"items/{fileName}"; // Puedes organizar en carpetas dentro del bucket

            try
            {
                // Convertir el archivo a un array de bytes
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Subir el archivo a Supabase Storage
                var storageResponse = await _supabaseClient.Storage
                    .From(bucketName)
                    .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                    {
                        Upsert = true, // Permite sobreescribir si ya existe un archivo con el mismo nombre
                        ContentType = imageFile.ContentType
                    });

                // Obtener la URL pública completa
                var publicUrl = _supabaseClient.Storage
                                                .From(bucketName)
                                                .GetPublicUrl(filePath);

                return publicUrl;
            }
            catch (Exception ex)
            {
                // Loguear la excepción de Supabase Storage
                // _logger.LogError(ex, "Error al subir imagen a Supabase Storage.");
                return null;
            }
        }

        /// <summary>
        /// Método auxiliar (opcional) para eliminar una imagen de Supabase Storage.
        /// Se recomienda si quieres evitar "imágenes huérfanas" al actualizar.
        /// </summary>
        /// <param name="imageUrl">La URL completa de la imagen a eliminar.</param>
        private async Task DeleteImageFromSupabase(string imageUrl)
        {
            var bucketName = _configuration["Supabase:BucketName"];
            if (string.IsNullOrEmpty(bucketName))
            {
                // Loguear error
                return;
            }

            try
            {
                // Parsear la URL para obtener el path del archivo dentro del bucket
                // Ejemplo de URL: https://[project_id].supabase.co/storage/v1/object/public/bucket_name/path/to/file.jpg
                // Necesitamos 'path/to/file.jpg'
                var uri = new Uri(imageUrl);
                var pathSegments = uri.Segments.Select(s => s.TrimEnd('/')).Skip(5).ToArray(); // Saltar /storage/v1/object/public/bucket_name/
                var filePathToDelete = string.Join("/", pathSegments);

                if (!string.IsNullOrEmpty(filePathToDelete))
                {
                    await _supabaseClient.Storage
                        .From(bucketName)
                        .Remove(new List<string> { filePathToDelete });
                }
            }
            catch (Exception ex)
            {
                // Loguear la excepción
                // _logger.LogError(ex, "Error al eliminar imagen de Supabase Storage con URL: {ImageUrl}", imageUrl);
            }
        }
    }
}
