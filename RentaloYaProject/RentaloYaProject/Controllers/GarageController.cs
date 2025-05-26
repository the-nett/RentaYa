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
                Disponible = dto.Status == "Disponible",
                ImageUrl = dto.ImageUrl

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


        // GET: /Garage/EditItem/{id}
        public async Task<IActionResult> EditItem(int id)
        {
            var itemToEdit = await _itemRepo.GetByIdAsync(id);
            if (itemToEdit == null)
            {
                return NotFound();
            }

            var viewModel = new CreateItemViewModel
            {
                Id = itemToEdit.Id, // MUY IMPORTANTE: Pasar el Id al ViewModel
                Name = itemToEdit.Name,
                Description = itemToEdit.Description,
                Price = itemToEdit.Price,
                ImageUrl = itemToEdit.ImageUrl,
                Location = itemToEdit.Location,
                Quantity = itemToEdit.QuantityAvailable,
                Category = itemToEdit.CategoryId.ToString(),
                RentType = itemToEdit.RentalTypeId.ToString()
            };

            await PopulateDropdowns(); // Este método pobla ViewData

            return View(viewModel);
        }

        // POST: /Garage/EditItem/{id}
        [HttpPost]
        [ValidateAntiForgeryToken] // Siempre recomendado para POST
        public async Task<IActionResult> EditItem(CreateItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Primero, recupera el ítem existente de la base de datos para obtener su URL de imagen actual
                var itemToUpdate = await _itemRepo.GetByIdAsync(model.Id);
                if (itemToUpdate == null)
                {
                    return NotFound();
                }

                string? currentImageUrl = itemToUpdate.ImageUrl; // Esta es la URL de la imagen que está actualmente en la DB

                string? newImageUrl = currentImageUrl; // Inicializa la URL final con la actual, por si no se sube nueva imagen

                // Si se seleccionó un nuevo archivo de imagen en el formulario
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Si ya existía una imagen asociada a este ítem, la eliminamos de Supabase
                    if (!string.IsNullOrEmpty(currentImageUrl))
                    {
                        var deleteSuccess = await DeleteImageFromSupabase(currentImageUrl);
                        if (!deleteSuccess)
                        {
                            Console.WriteLine($"Advertencia: No se pudo eliminar la imagen antigua de Supabase: {currentImageUrl}. Continuar con la subida de la nueva.");
                            // Aquí podrías decidir si quieres que esto sea un error fatal o solo una advertencia.
                            // Por ahora, asumimos que la subida de la nueva imagen es más importante.
                        }
                    }

                    // Ahora, subimos la nueva imagen a Supabase
                    newImageUrl = await UploadImageToSupabase(model.ImageFile);

                    if (string.IsNullOrEmpty(newImageUrl))
                    {
                        ModelState.AddModelError("ImageFile", "Error al subir la nueva imagen a Supabase.");
                        await PopulateDropdowns(); // Rellena las listas para que la vista no falle
                        return View(model);
                    }
                }
                // Si model.ImageFile es null, significa que el usuario no seleccionó una nueva imagen.
                // En ese caso, newImageUrl ya es igual a currentImageUrl (la URL existente de la DB).

                // Mapear los datos del ViewModel al modelo de la base de datos (Item)
                itemToUpdate.Name = model.Name;
                itemToUpdate.Description = model.Description;
                itemToUpdate.Price = model.Price;
                itemToUpdate.ImageUrl = newImageUrl; // Asigna la URL final (nueva o la antigua si no hubo cambio)
                itemToUpdate.Location = model.Location;
                itemToUpdate.QuantityAvailable = model.Quantity;
                itemToUpdate.CategoryId = int.Parse(model.Category); // Asumo que Category y RentType son IDs de string que necesitan parseo
                itemToUpdate.RentalTypeId = int.Parse(model.RentType);

                await _itemRepo.UpdateAsync(itemToUpdate);
                // No necesitas await _itemRepo.Save() aquí si UpdateAsync ya persiste los cambios
                // o si Save() se llama de forma global en tu Unit of Work/Patrón.

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns();
            return View(model);
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

        // Este método toma la URL COMPLETA de la imagen para eliminarla.
        private async Task<bool> DeleteImageFromSupabase(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false; // No hay URL para eliminar
            }

            var bucketName = _configuration["Supabase:BucketName"];
            try
            {
                var uri = new Uri(imageUrl);
                // La URL de Supabase es algo como: https://[project].supabase.co/storage/v1/object/public/[bucket]/[path/to/file.ext]
                // Necesitamos extraer "[path/to/file.ext]"
                var publicPathSegment = $"/storage/v1/object/public/{bucketName}/";
                var relativePath = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(publicPathSegment) + publicPathSegment.Length);

                if (string.IsNullOrEmpty(relativePath))
                {
                    Console.WriteLine($"Error: No se pudo extraer la ruta del archivo de la URL: {imageUrl}");
                    return false;
                }

                // Supabase Storage Remove requiere un array de nombres de archivos/rutas
                var response = await _supabaseClient.Storage
                    .From(bucketName)
                    .Remove(new List<string> { relativePath });

                return response != null && response.Any(); // Si la respuesta no es nula y contiene elementos, la eliminación fue exitosa
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception during image deletion: {ex.Message}");
                return false;
            }
        }
    }
}
