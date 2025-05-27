//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using RentaloYa.Application.Common.DTOs;
//using RentaloYa.Application.Common.Interfaces;
//using RentaloYa.Application.Services.InterfacesServices;
//using RentaloYa.Domain.Entities;
//using RentaloYa.Infrastructure.Repository;
//using RentaloYa.Web.ViewModels.Garage;
//using RentalWeb.Web.ViewModels.Garage;
//using Supabase.Interfaces;
//using System.Security.Claims;

//namespace RentalWeb.Web.Controllers
//{
//    [Authorize]
//    public class GarageController : Controller
//    {
//        private readonly IItemService _itemService;
//        private readonly IRepository<Category> _categoryRepo;
//        private readonly IRepository<Item> _itemRepo;
//        private readonly IRepository<RentalType> _rentalTypeRepo;
//        private readonly IUserRepository _userRepository;
//        private readonly IConfiguration _configuration;
//        private readonly Supabase.Client _supabaseClient;

//        public GarageController(
//            IItemService itemService,
//            IRepository<Category> categoryRepo,
//            IRepository<RentalType> rentalTypeRepo,
//            IUserRepository userRepository,
//            IRepository<Item> itemRepo,
//            IConfiguration configuration,
//            Supabase.Client supabaseClient)
//        {
//            _itemService = itemService;
//            _categoryRepo = categoryRepo;
//            _rentalTypeRepo = rentalTypeRepo;
//            _userRepository = userRepository;
//            _itemRepo = itemRepo;
//            _configuration = configuration;
//            _supabaseClient = supabaseClient;
//        }

//        // -------- LISTADO PRINCIPAL ----------
//        [HttpGet]
//        public async Task<IActionResult> Index()
//        {
//            var correo = User.FindFirstValue(ClaimTypes.Email);

//            Console.WriteLine($"[DEBUG] User e-mail para listado: {correo}");

//            var dtos = await _itemService.GetItemsByUserEmailAsync(correo);

//            var viewModel = dtos.Select(dto => new GarageItemViewModel
//            {
//                Id = dto.Id,
//                Nombre = dto.Name,
//                Cantidad = dto.QuantityAvailable,
//                TipoRenta = dto.RentalTypeName,
//                Disponible = dto.Status,
//                ImageUrl = dto.ImageUrl
//            }).ToList();

//            return View(viewModel);
//        }

//        // -------- FORMULARIO NUEVO ITEM ----------
//        [HttpGet]
//        public async Task<IActionResult> AddItem()
//        {
//            await PopulateDropdowns();
//            return View(new CreateItemViewModel());
//        }

//        // -------- CREACIÓN ITEM ----------
//        [HttpPost, ActionName("AddItem")]
//        [ValidateAntiForgeryToken]
//        //public async Task<IActionResult> CreateItem(CreateItemViewModel model)
//        //{
//        //    try
//        //    {
//        //        if (!ModelState.IsValid)
//        //        {
//        //            Console.WriteLine("[WARNING] ModelState inválido.");
//        //            await PopulateDropdowns();
//        //            return View("AddItem", model);
//        //        }

//        //        // ---------- Subir imagen ----------
//        //        string? imageUrl = null;
//        //        if (model.ImageFile != null)
//        //        {
//        //            Console.WriteLine($"[DEBUG] Archivo recibido: {model.ImageFile.FileName} | Size: {model.ImageFile.Length}");
//        //            imageUrl = await UploadImageToSupabase(model.ImageFile);

//        //            if (string.IsNullOrEmpty(imageUrl))
//        //            {
//        //                Console.WriteLine("[ERROR] UploadImageToSupabase devolvió null.");
//        //                ModelState.AddModelError("ImageFile", "Error al subir la imagen a Supabase.");
//        //                await PopulateDropdowns();
//        //                return View("AddItem", model);
//        //            }
//        //        }
//        //        else
//        //        {
//        //            Console.WriteLine("[WARNING] model.ImageFile es null.");
//        //        }

//        //        model.ImageUrl = imageUrl;

//        //        // ---------- Crear objeto Item ----------
//        //        var correo = User.FindFirstValue(ClaimTypes.Email);
//        //        var userCurrent = await _userRepository.GetUserByEmailAsync(correo);

//        //        var newItem = new Item
//        //        {
//        //            OwnerId = userCurrent.Id,
//        //            Name = model.Name,
//        //            Description = model.Description,
//        //            RentalTypeId = int.Parse(model.RentType),
//        //            Price = model.Price,
//        //            ImageUrl = model.ImageUrl,
//        //            CategoryId = int.Parse(model.Category),
//        //            Location = model.Location,
//        //            ItemStatusId = 1,
//        //            QuantityAvailable = model.Quantity,
//        //            CreatedAt = DateTime.UtcNow
//        //        };

//        //        await _itemRepo.AddAsync(newItem);
//        //        Console.WriteLine($"[DEBUG] Item {newItem.Name} guardado con ID {newItem.Id}");

//        //        TempData["SuccessMessage"] = "Artículo añadido exitosamente.";
//        //        return RedirectToAction("Index");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"[ERROR] Excepción en CreateItem: {ex}");
//        //        ModelState.AddModelError("", "Ocurrió un error al intentar añadir el artículo.");
//        //        await PopulateDropdowns();
//        //        return View("AddItem", model);
//        //    }
//        //}
//        // -------- CREACIÓN ITEM (ACTUALIZADO) ----------
//        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    Console.WriteLine("[WARNING] ModelState inválido en CreateItem.");
//                    await PopulateDropdowns();
//                    return View("AddItem", model);
//                }

//                // 1. Convertir IFormFile a byte[]
//                byte[]? imageData = null;
//                if (model.ImageFile != null && model.ImageFile.Length > 0)
//                {
//                    using var ms = new MemoryStream();
//                    await model.ImageFile.CopyToAsync(ms);
//                    imageData = ms.ToArray();
//                    Console.WriteLine($"[DEBUG] Imagen convertida a byte[] de tamaño: {imageData.Length}");
//                }
//                else
//                {
//                    // Si la imagen es obligatoria, añade un error al ModelState
//                    ModelState.AddModelError("ImageFile", "La imagen es obligatoria.");
//                    await PopulateDropdowns();
//                    return View("AddItem", model);
//                }

//                // 2. Mapear CreateItemViewModel a CreateItemDto manualmente
//                var createItemDto = new CreateItemDto
//                {
//                    Name = model.Name,
//                    Description = model.Description,
//                    RentalTypeId = int.Parse(model.RentType), // Asegúrate de que el ViewModel pase un string que pueda ser parseado a int
//                    Price = model.Price,
//                    CategoryId = int.Parse(model.Category), // Asegúrate de que el ViewModel pase un string que pueda ser parseado a int
//                    Location = model.Location,
//                    Quantity = model.Quantity,
//                    ImageData = imageData // Aquí asignamos el byte[] de la imagen
//                };

//                // 3. Obtener el email del usuario logueado
//                var userEmail = User.FindFirstValue(ClaimTypes.Email);
//                if (string.IsNullOrEmpty(userEmail))
//                {
//                    ModelState.AddModelError("", "No se pudo obtener el email del usuario logueado.");
//                    await PopulateDropdowns();
//                    return View("AddItem", model);
//                }

//                // 4. Crear la función (delegado) para subir la imagen que se pasará al servicio
//                // Esta función envuelve tu método UploadImageToSupabase, pero para el servicio
//                // solo debe aceptar byte[] y devolver la URL.
//                Func<byte[], Task<string?>> uploadImageFunc = async (bytes) =>
//                {
//                    // Aquí llamamos a tu método existente UploadImageToSupabase
//                    // Necesitará el nombre del archivo original y el tipo de contenido para el bucket
//                    // Los obtenemos del IFormFile original del modelo.
//                    return await UploadImageToSupabase(bytes, model.ImageFile!.FileName, model.ImageFile.ContentType);
//                };

//                // 5. Llamar a _itemService.CreateItemWithTagsAsync()
//                var (success, errorMessage, createdItemId) = await _itemService.CreateItemWithTagsAsync(createItemDto, userEmail, uploadImageFunc);

//                if (success)
//                {
//                    TempData["SuccessMessage"] = "Artículo añadido exitosamente y etiquetas generadas.";
//                    return RedirectToAction("Index");
//                }
//                else
//                {
//                    Console.WriteLine($"[ERROR] Error al crear ítem con etiquetas: {errorMessage}");
//                    ModelState.AddModelError("", errorMessage ?? "Ocurrió un error desconocido al añadir el artículo.");
//                    await PopulateDropdowns();
//                    return View("AddItem", model);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[ERROR] Excepción en CreateItem (con Gemini): {ex}");
//                ModelState.AddModelError("", "Ocurrió un error inesperado al intentar añadir el artículo.");
//                await PopulateDropdowns();
//                return View("AddItem", model);
//            }
//        }

//        // -------- UPLOAD A SUPABASE ----------
//        private async Task<string?> UploadImageToSupabase(IFormFile imageFile)
//        {
//            var bucketName = _configuration["Supabase:BucketName"];
//            if (string.IsNullOrEmpty(bucketName))
//            {
//                Console.WriteLine("[ERROR] BucketName no configurado.");
//                return null;
//            }

//            var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(imageFile.FileName)}";
//            var filePath = $"items/{fileName}";

//            Console.WriteLine($"[DEBUG] Subiendo a bucket: {bucketName} | Path: {filePath}");

//            try
//            {
//                using var ms = new MemoryStream();
//                await imageFile.CopyToAsync(ms);
//                var bytes = ms.ToArray();

//                Console.WriteLine($"[DEBUG] Bytes a subir: {bytes.Length}");
//                Console.WriteLine($"[DEBUG] Token visible para Supabase: {_supabaseClient.Auth.CurrentSession?.AccessToken?.Substring(0, 20) ?? "NULL"}");
//                Console.WriteLine($"[DEBUG] User ID Supabase: {_supabaseClient.Auth.CurrentUser?.Id ?? "NULL"}");
//                var uploadResult = await _supabaseClient.Storage
//                    .From(bucketName)

//                    .Upload(bytes, filePath, new Supabase.Storage.FileOptions
//                    {

//                        Upsert = true,
//                        ContentType = imageFile.ContentType
//                    });

//                Console.WriteLine($"[DEBUG] Resultado Supabase.Upload: {uploadResult}");

//                var publicUrl = _supabaseClient.Storage.From(bucketName).GetPublicUrl(filePath);
//                Console.WriteLine($"[DEBUG] URL pública generada: {publicUrl}");
//                return publicUrl;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[ERROR] Excepción al subir a Supabase: {ex.Message}");
//                return null;
//            }
//        }

//        // Este método toma la URL COMPLETA de la imagen para eliminarla.
//        private async Task<bool> DeleteImageFromSupabase(string imageUrl)
//        {
//            if (string.IsNullOrEmpty(imageUrl))
//            {
//                return false; // No hay URL para eliminar
//            }

//            var bucketName = _configuration["Supabase:BucketName"];
//            try
//            {
//                var uri = new Uri(imageUrl);
//                // La URL de Supabase es algo como: https://[project].supabase.co/storage/v1/object/public/[bucket]/[path/to/file.ext]
//                // Necesitamos extraer "[path/to/file.ext]"
//                var publicPathSegment = $"/storage/v1/object/public/{bucketName}/";
//                var relativePath = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(publicPathSegment) + publicPathSegment.Length);

//                if (string.IsNullOrEmpty(relativePath))
//                {
//                    Console.WriteLine($"Error: No se pudo extraer la ruta del archivo de la URL: {imageUrl}");
//                    return false;
//                }

//                // Supabase Storage Remove requiere un array de nombres de archivos/rutas
//                var response = await _supabaseClient.Storage
//                    .From(bucketName)
//                    .Remove(new List<string> { relativePath });

//                return response != null && response.Any(); // Si la respuesta no es nula y contiene elementos, la eliminación fue exitosa
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"General Exception during image deletion: {ex.Message}");
//                return false;
//            }
//        }
//        // ---------- Helpers ----------
//        private async Task PopulateDropdowns()
//        {
//            var categories = await _categoryRepo.GetAllAsync();
//            var rentalTypes = await _rentalTypeRepo.GetAllAsync();

//            ViewData["CategoryList"] = categories.Select(c => new SelectListItem
//            {
//                Text = c.Name,
//                Value = c.Id.ToString()
//            }).ToList();

//            ViewData["RentalTypeList"] = rentalTypes.Select(rt => new SelectListItem
//            {
//                Text = rt.TypeName,
//                Value = rt.Id.ToString()
//            }).ToList();
//        }
//        // GET: /Garage/EditItem/{id}
//        public async Task<IActionResult> EditItem(int id)
//        {
//            var itemToEdit = await _itemRepo.GetByIdAsync(id);
//            if (itemToEdit == null)
//            {
//                return NotFound();
//            }

//            var viewModel = new CreateItemViewModel
//            {
//                Id = itemToEdit.Id, // MUY IMPORTANTE: Pasar el Id al ViewModel
//                Name = itemToEdit.Name,
//                Description = itemToEdit.Description,
//                Price = itemToEdit.Price,
//                ImageUrl = itemToEdit.ImageUrl,
//                Location = itemToEdit.Location,
//                Quantity = itemToEdit.QuantityAvailable,
//                Category = itemToEdit.CategoryId.ToString(),
//                RentType = itemToEdit.RentalTypeId.ToString()
//            };

//            await PopulateDropdowns(); // Este método pobla ViewData

//            return View(viewModel);
//        }

//        // POST: /Garage/EditItem/{id}
//        [HttpPost]
//        [ValidateAntiForgeryToken] // Siempre recomendado para POST
//        public async Task<IActionResult> EditItem(CreateItemViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Primero, recupera el ítem existente de la base de datos para obtener su URL de imagen actual
//                var itemToUpdate = await _itemRepo.GetByIdAsync(model.Id);
//                if (itemToUpdate == null)
//                {
//                    return NotFound();
//                }

//                string? currentImageUrl = itemToUpdate.ImageUrl; // Esta es la URL de la imagen que está actualmente en la DB

//                string? newImageUrl = currentImageUrl; // Inicializa la URL final con la actual, por si no se sube nueva imagen

//                // Si se seleccionó un nuevo archivo de imagen en el formulario
//                if (model.ImageFile != null && model.ImageFile.Length > 0)
//                {
//                    // Si ya existía una imagen asociada a este ítem, la eliminamos de Supabase
//                    if (!string.IsNullOrEmpty(currentImageUrl))
//                    {
//                        var deleteSuccess = await DeleteImageFromSupabase(currentImageUrl);
//                        if (!deleteSuccess)
//                        {
//                            Console.WriteLine($"Advertencia: No se pudo eliminar la imagen antigua de Supabase: {currentImageUrl}. Continuar con la subida de la nueva.");
//                            // Aquí podrías decidir si quieres que esto sea un error fatal o solo una advertencia.
//                            // Por ahora, asumimos que la subida de la nueva imagen es más importante.
//                        }
//                    }

//                    // Ahora, subimos la nueva imagen a Supabase
//                    newImageUrl = await UploadImageToSupabase(model.ImageFile);

//                    if (string.IsNullOrEmpty(newImageUrl))
//                    {
//                        ModelState.AddModelError("ImageFile", "Error al subir la nueva imagen a Supabase.");
//                        await PopulateDropdowns(); // Rellena las listas para que la vista no falle
//                        return View(model);
//                    }
//                }
//                // Si model.ImageFile es null, significa que el usuario no seleccionó una nueva imagen.
//                // En ese caso, newImageUrl ya es igual a currentImageUrl (la URL existente de la DB).

//                // Mapear los datos del ViewModel al modelo de la base de datos (Item)
//                itemToUpdate.Name = model.Name;
//                itemToUpdate.Description = model.Description;
//                itemToUpdate.Price = model.Price;
//                itemToUpdate.ImageUrl = newImageUrl; // Asigna la URL final (nueva o la antigua si no hubo cambio)
//                itemToUpdate.Location = model.Location;
//                itemToUpdate.QuantityAvailable = model.Quantity;
//                itemToUpdate.CategoryId = int.Parse(model.Category); // Asumo que Category y RentType son IDs de string que necesitan parseo
//                itemToUpdate.RentalTypeId = int.Parse(model.RentType);

//                await _itemRepo.UpdateAsync(itemToUpdate);
//                // No necesitas await _itemRepo.Save() aquí si UpdateAsync ya persiste los cambios
//                // o si Save() se llama de forma global en tu Unit of Work/Patrón.

//                return RedirectToAction(nameof(Index));
//            }

//            await PopulateDropdowns();
//            return View(model);
//        }
//        [HttpPost] // Es más seguro usar HttpPost para operaciones de eliminación
//        [ValidateAntiForgeryToken] // Siempre usar para POSTs que modifican datos
//        public async Task<IActionResult> DeleteItem(int id)
//        {
//            try
//            {
//                await _itemRepo.DeleteAsync(id);

//                TempData["SuccessMessage"] = "Artículo eliminado exitosamente.";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                // Aquí deberías loguear la excepción para depuración
//                // _logger.LogError(ex, "Error al eliminar el artículo con ID {ItemId}", id);

//                TempData["ErrorMessage"] = "Ocurrió un error al intentar eliminar el artículo.";
//                return RedirectToAction("Index"); // O a una página de error
//            }
//        }


//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces; // Para IUserRepository, y potencialmente IItemRepository si la inyectas aquí
using RentaloYa.Application.Services.InterfacesServices; // Para IItemService
using RentaloYa.Domain.Entities;
// using RentaloYa.Infrastructure.Repository; // <--- SE PUEDE ELIMINAR O COMENTAR si ya no usas ItemRepository directamente
using RentaloYa.Web.ViewModels.Garage;
using RentalWeb.Web.ViewModels.Garage; // Asegúrate de que esta es la ruta correcta si tu ViewModel está en otro proyecto/namespace
using Supabase.Interfaces; // Asegúrate de que este es el using correcto para Supabase.Client
using System.Security.Claims;
using System.IO;
using RentaloYa.Infrastructure.Repository; // ¡Nuevo using necesario para MemoryStream y Path!

namespace RentalWeb.Web.Controllers
{
    [Authorize]
    public class GarageController : Controller
    {
        private readonly IItemService _itemService;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Item> _itemRepo; // Este es para el IRepository<Item> genérico
        private readonly IRepository<RentalType> _rentalTypeRepo;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly Supabase.Client _supabaseClient;

        // Si necesitas IItemRepository para métodos específicos que no estén en IRepository<Item>,
        // DEBERÍAS INYECTARLA AQUÍ Y EN EL CONSTRUCTOR. Por ahora, asumo que _itemRepo es suficiente
        // para las operaciones básicas que no maneja _itemService.
        // private readonly IItemRepository _extendedItemRepository; 

        public GarageController(
            IItemService itemService,
            IRepository<Category> categoryRepo,
            IRepository<RentalType> rentalTypeRepo,
            IUserRepository userRepository,
            IRepository<Item> itemRepo, // Para operaciones genéricas de Item (Edit, Delete, GetById)
            IConfiguration configuration,
            Supabase.Client supabaseClient)
        {
            _itemService = itemService;
            _categoryRepo = categoryRepo;
            _rentalTypeRepo = rentalTypeRepo;
            _userRepository = userRepository;
            _itemRepo = itemRepo;
            _configuration = configuration;
            _supabaseClient = supabaseClient;

            // Si inyectas IItemRepository específica, la asignarías aquí:
            // _extendedItemRepository = extendedItemRepository;
        }

        // -------- LISTADO PRINCIPAL ----------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email);

            Console.WriteLine($"[DEBUG] User e-mail para listado: {correo}");

            var dtos = await _itemService.GetItemsByUserEmailAsync(correo);

            var viewModel = dtos.Select(dto => new GarageItemViewModel
            {
                Id = dto.Id,
                Nombre = dto.Name,
                Cantidad = dto.QuantityAvailable,
                TipoRenta = dto.RentalTypeName,
                Disponible = dto.Status,
                ImageUrl = dto.ImageUrl
            }).ToList();

            return View(viewModel);
        }

        // -------- FORMULARIO NUEVO ITEM ----------
        [HttpGet]
        public async Task<IActionResult> AddItem()
        {
            await PopulateDropdowns();
            return View(new CreateItemViewModel());
        }

        // -------- CREACIÓN ITEM ----------
        [HttpPost, ActionName("AddItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("[WARNING] ModelState inválido en CreateItem.");
                    await PopulateDropdowns();
                    return View("AddItem", model);
                }

                // 1. Convertir IFormFile a byte[]
                byte[]? imageData = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ImageFile.CopyToAsync(ms);
                    imageData = ms.ToArray();
                    Console.WriteLine($"[DEBUG] Imagen convertida a byte[] de tamaño: {imageData.Length}");
                }
                else
                {
                    // Si la imagen es obligatoria, añade un error al ModelState
                    ModelState.AddModelError("ImageFile", "La imagen es obligatoria.");
                    await PopulateDropdowns();
                    return View("AddItem", model);
                }

                // 2. Mapear CreateItemViewModel a CreateItemDto manualmente
                var createItemDto = new CreateItemDto
                {
                    Name = model.Name,
                    Description = model.Description,
                    // ASUMO que ya CAMBIASTE el DTO para que CategoryId y RentalTypeId sean int
                    // Si no lo hiciste, int.Parse() es correcto aquí, pero lo ideal es que el DTO ya espere INT.
                    RentalTypeId = int.Parse(model.RentType),
                    Price = model.Price,
                    CategoryId = int.Parse(model.Category),
                    Location = model.Location,
                    Quantity = model.Quantity,
                    ImageData = imageData // Aquí asignamos el byte[] de la imagen
                };

                // 3. Obtener el email del usuario logueado
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                {
                    ModelState.AddModelError("", "No se pudo obtener el email del usuario logueado.");
                    await PopulateDropdowns();
                    return View("AddItem", model);
                }

                // 4. Crear la función (delegado) para subir la imagen que se pasará al servicio
                // Esta función envuelve tu método UploadImageToSupabase, pero para el servicio
                // solo debe aceptar byte[] y devolver la URL.
                Func<byte[], Task<string?>> uploadImageFunc = async (bytes) =>
                {
                    // Aquí llamamos a tu método existente UploadImageToSupabase
                    // Necesitará el nombre del archivo original y el tipo de contenido para el bucket
                    // Los obtenemos del IFormFile original del modelo.
                    // ¡Importante!: model.ImageFile! (null-forgiving operator) asume que ImageFile no será null aquí
                    // porque ya lo validamos arriba.
                    return await UploadImageToSupabase(bytes, model.ImageFile!.FileName, model.ImageFile.ContentType);
                };

                // 5. Llamar a _itemService.CreateItemWithTagsAsync()
                var (success, errorMessage, createdItemId) = await _itemService.CreateItemWithTagsAsync(createItemDto, userEmail, uploadImageFunc);

                if (success)
                {
                    TempData["SuccessMessage"] = "Artículo añadido exitosamente y etiquetas generadas.";
                    return RedirectToAction("Index");
                }
                else
                {
                    Console.WriteLine($"[ERROR] Error al crear ítem con etiquetas: {errorMessage}");
                    ModelState.AddModelError("", errorMessage ?? "Ocurrió un error desconocido al añadir el artículo.");
                    await PopulateDropdowns();
                    return View("AddItem", model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en CreateItem (con Gemini): {ex}");
                ModelState.AddModelError("", "Ocurrió un error inesperado al intentar añadir el artículo.");
                await PopulateDropdowns();
                return View("AddItem", model);
            }
        }

        // -------- UPLOAD A SUPABASE (NUEVA SOBRECARGA) ----------
        // Este método acepta byte[] y los detalles del archivo para la subida.
        // Será llamado por el delegado 'uploadImageFunc'.
        private async Task<string?> UploadImageToSupabase(byte[] imageBytes, string fileName, string contentType)
        {
            var bucketName = _configuration["Supabase:BucketName"];
            if (string.IsNullOrEmpty(bucketName))
            {
                Console.WriteLine("[ERROR] BucketName no configurado.");
                return null;
            }

            var uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(fileName)}";
            var filePath = $"items/{uniqueFileName}";

            Console.WriteLine($"[DEBUG] Subiendo a bucket: {bucketName} | Path: {filePath}");

            try
            {
                Console.WriteLine($"[DEBUG] Bytes a subir: {imageBytes.Length}");
                Console.WriteLine($"[DEBUG] Token visible para Supabase: {_supabaseClient.Auth.CurrentSession?.AccessToken?.Substring(0, 20) ?? "NULL"}");
                Console.WriteLine($"[DEBUG] User ID Supabase: {_supabaseClient.Auth.CurrentUser?.Id ?? "NULL"}");

                var uploadResult = await _supabaseClient.Storage
                    .From(bucketName)
                    .Upload(imageBytes, filePath, new Supabase.Storage.FileOptions
                    {
                        Upsert = true,
                        ContentType = contentType
                    });

                Console.WriteLine($"[DEBUG] Resultado Supabase.Upload: {uploadResult}");

                var publicUrl = _supabaseClient.Storage.From(bucketName).GetPublicUrl(filePath);
                Console.WriteLine($"[DEBUG] URL pública generada: {publicUrl}");
                return publicUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción al subir a Supabase (desde bytes): {ex.Message}");
                return null;
            }
        }

        // -------- UPLOAD A SUPABASE (MÉTODO ORIGINAL - AHORA LLAMA A LA SOBRECARGA) ----------
        // Este método se mantiene para compatibilidad con la lógica existente que usa IFormFile.
        // Convertirá el IFormFile a bytes y delegará la subida a la nueva sobrecarga.
        private async Task<string?> UploadImageToSupabase(IFormFile imageFile)
        {
            if (imageFile == null) return null; // Manejo de null para IFormFile

            using var ms = new MemoryStream();
            await imageFile.CopyToAsync(ms);
            var bytes = ms.ToArray();
            // Llama a la nueva sobrecarga con los detalles del archivo
            return await UploadImageToSupabase(bytes, imageFile.FileName, imageFile.ContentType);
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

        // ---------- Helpers ----------
        private async Task PopulateDropdowns()
        {
            var categories = await _categoryRepo.GetAllAsync();
            var rentalTypes = await _rentalTypeRepo.GetAllAsync();

            ViewData["CategoryList"] = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();

            ViewData["RentalTypeList"] = rentalTypes.Select(rt => new SelectListItem
            {
                Text = rt.TypeName,
                Value = rt.Id.ToString()
            }).ToList();
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
                    // Usamos la sobrecarga que acepta IFormFile directamente
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

        [HttpPost] // Es más seguro usar HttpPost para operaciones de eliminación
        [ValidateAntiForgeryToken] // Siempre usar para POSTs que modifican datos
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                // Antes de eliminar el ítem, intenta eliminar su imagen si existe
                var itemToDelete = await _itemRepo.GetByIdAsync(id);
                if (itemToDelete != null && !string.IsNullOrEmpty(itemToDelete.ImageUrl))
                {
                    var deleteImageSuccess = await DeleteImageFromSupabase(itemToDelete.ImageUrl);
                    if (!deleteImageSuccess)
                    {
                        Console.WriteLine($"Advertencia: No se pudo eliminar la imagen de Supabase para el ítem {id}: {itemToDelete.ImageUrl}. El ítem será eliminado de la DB de todos modos.");
                        // Puedes decidir si quieres que esto impida la eliminación del ítem o no.
                    }
                }

                await _itemRepo.DeleteAsync(id);

                TempData["SuccessMessage"] = "Artículo eliminado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Aquí deberías loguear la excepción para depuración
                Console.WriteLine($"[ERROR] Error al eliminar el artículo con ID {id}: {ex.Message}");
                // _logger.LogError(ex, "Error al eliminar el artículo con ID {ItemId}", id);

                TempData["ErrorMessage"] = "Ocurrió un error al intentar eliminar el artículo.";
                return RedirectToAction("Index"); // O a una página de error
            }
        }
    }
}