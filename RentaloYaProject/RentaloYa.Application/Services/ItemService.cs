using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces; // ESTE USING ES CRÍTICO para IItemRepository, IImageTaggingService, IUserRepository
using RentaloYa.Application.Services.InterfacesServices; // Para IItemService (si está en su propia interfaz)
using RentaloYa.Domain.Entities; // Para Item, Tag, ItemTag, etc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentaloYa.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IImageTaggingService _imageTaggingService;
        private readonly IUserRepository _userRepository;

        public ItemService(IItemRepository itemRepository,
                           IImageTaggingService imageTaggingService,
                           IUserRepository userRepository)
        {
            _itemRepository = itemRepository;
            _imageTaggingService = imageTaggingService;
            _userRepository = userRepository;
        }

        public async Task<List<DetailedItemDto>> GetItemsByUserEmailAsync(string email)
        {
            var items = await _itemRepository.GetItemsByUserEmailAsync(email);

            return items.Select(item => new DetailedItemDto
            {
                Id = item.Id,
                Name = item.Name,
                QuantityAvailable = item.QuantityAvailable,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                CategoryName = item.Category?.Name,     // Uso el operador de propagación de nulos (?) para mayor seguridad
                RentalTypeName = item.RentalType?.TypeName, // Uso el operador de propagación de nulos (?)
                Status = item.ItemStatus?.StatusName    // Uso el operador de propagación de nulos (?)
            }).ToList();
        }

        // --- IMPLEMENTACIÓN DEL MÉTODO PARA CREAR UN ITEM CON IMAGEN Y ETIQUETAS ---
        public async Task<(bool Success, string? ErrorMessage, int? CreatedItemId)> CreateItemWithTagsAsync(
            CreateItemDto itemDto,
            string userEmail,
            Func<byte[], Task<string?>> uploadImageFunc)
        {
            try
            {
                string? imageUrl = null;
                List<ImageTaggingResult>? imageTags = null; // Para almacenar los resultados de Gemini

                // 1. Procesar la imagen (analizar con Gemini y subir a Supabase)
                if (itemDto.ImageData != null && itemDto.ImageData.Length > 0)
                {
                    // Analizar la imagen con Gemini (ya viene como byte[])
                    imageTags = (await _imageTaggingService.GetTagsFromImageAsync(itemDto.ImageData))?.ToList();

                    if (imageTags == null || !imageTags.Any())
                    {
                        Console.WriteLine("[WARNING] Gemini no devolvió etiquetas o la lista está vacía. El artículo se creará sin tags.");
                    }

                    // Subir la imagen a Supabase (delegando al controlador con byte[])
                    imageUrl = await uploadImageFunc(itemDto.ImageData);

                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        Console.WriteLine("[ERROR] Error al subir la imagen a Supabase (delegado).");
                        return (false, "Error al subir la imagen.", null);
                    }
                }
                else
                {
                    Console.WriteLine("[WARNING] No se proporcionó archivo de imagen para el artículo.");
                    // Dependiendo de tu lógica, esto podría ser un error fatal si la imagen es obligatoria.
                    // Actualmente, si no hay imagen, imageUrl será null.
                }

                // Obtener el ID del usuario propietario del item
                var userCurrent = await _userRepository.GetUserByEmailAsync(userEmail);
                if (userCurrent == null)
                {
                    Console.WriteLine($"[ERROR] Usuario no encontrado para email: {userEmail}");
                    return (false, "Usuario no encontrado.", null);
                }

                // 2. Crear el objeto Item a partir del DTO
                var newItem = new Item
                {
                    OwnerId = userCurrent.Id,
                    Name = itemDto.Name,
                    Description = itemDto.Description,
                    RentalTypeId = itemDto.RentalTypeId,
                    Price = itemDto.Price,
                    CategoryId = itemDto.CategoryId,
                    Location = itemDto.Location,
                    ItemStatusId = 1, // Asumo que 1 es 'disponible' o el estado inicial
                    QuantityAvailable = itemDto.Quantity,
                    ImageUrl = imageUrl, // Asigna la URL de la imagen aquí
                    CreatedAt = DateTime.UtcNow
                };

                // Añadir el Item al contexto a través del ItemRepository
                await _itemRepository.AddAsync(newItem);
                // NOTA: NO LLAMAMOS SaveChangesAsync() AQUÍ. El ID de newItem aún no ha sido asignado por la DB.

                // 3. Procesar y guardar las etiquetas si existen
                if (imageTags != null && imageTags.Any())
                {
                    var itemTagsToAdd = new List<ItemTag>();
                    foreach (var tagResult in imageTags)
                    {
                        // Buscar si el tag ya existe por nombre
                        var existingTag = await _itemRepository.GetTagByNameAsync(tagResult.Name);

                        if (existingTag == null)
                        {
                            // Si el tag no existe, crearlo
                            existingTag = new Tag { Name = tagResult.Name };
                            await _itemRepository.AddTagAsync(existingTag);
                            // NOTA: NO LLAMAMOS SaveChangesAsync() AQUÍ. El ID del nuevo Tag aún no ha sido asignado por la DB.
                        }

                        // --- CORRECCIÓN CLAVE AQUÍ: Asignar las entidades de navegación en lugar de los IDs ---
                        var itemTag = new ItemTag
                        {
                            Item = newItem,        // <--- ¡Asigna la entidad Item completa!
                            Tag = existingTag,     // <--- ¡Asigna la entidad Tag completa!
                            Confidence = tagResult.Confidence
                        };
                        itemTagsToAdd.Add(itemTag);
                    }
                    if (itemTagsToAdd.Any())
                    {
                        await _itemRepository.AddItemTagsAsync(itemTagsToAdd);
                        // NOTA: NO LLAMAMOS SaveChangesAsync() AQUÍ.
                    }
                }

                // 4. Guardar TODOS los cambios pendientes en una única transacción
                // Este SaveChangesAsync es CRÍTICO para que newItem.Id y existingTag.TagId (si son nuevos tags) se persistan
                // y para que las relaciones ItemTag se guarden correctamente.
                await _itemRepository.SaveChangesAsync();

                // Ahora, newItem.Id debería tener el ID real generado por la base de datos.
                return (true, null, newItem.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Excepción en ItemService.CreateItemWithTagsAsync: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");
                return (false, $"Error al crear el artículo: {ex.Message}", null);
            }
        }
    }
}