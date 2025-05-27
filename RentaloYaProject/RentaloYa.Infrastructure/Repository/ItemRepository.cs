using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces; // Para IItemRepository
using RentaloYa.Domain.Entities; // Para Item, Tag, ItemTag, etc.
using RentaloYa.Infrastructure.Data; // Tu DbContext (ApplicationDbContext)

namespace RentaloYa.Infrastructure.Repository
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------ Métodos de Item ------------------------------

        /// <summary>
        /// Añade un nuevo Item a la base de datos.
        /// </summary>
        /// <param name="item">El objeto Item a añadir.</param>
        public async Task AddAsync(Item item)
        {
            await _context.Items.AddAsync(item);
            // IMPORTANTE: NO LLAMAMOS SaveChangesAsync AQUÍ.
            // La llamada a SaveChangesAsync se hará de forma explícita en el ItemService.
        }

        /// <summary>
        /// Obtiene una lista de Items asociados a un usuario por su dirección de email.
        /// </summary>
        /// <param name="email">El email del usuario propietario.</param>
        /// <returns>Una lista de objetos Item.</returns>
        public async Task<List<Item>> GetItemsByUserEmailAsync(string email)
        {
            return await _context.Items
                .Include(i => i.Owner)       // Incluye la entidad Owner si necesitas sus datos
                .Include(i => i.Category)
                .Include(i => i.RentalType)
                .Include(i => i.ItemStatus)
                .Where(i => i.Owner.Email == email /* && !i.IsDeleted */) // Descomenta si manejas soft delete
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una lista de Items asociados a un usuario por su ID.
        /// </summary>
        /// <param name="userId">El ID del usuario propietario.</param>
        /// <returns>Una lista de objetos Item.</returns>
        public async Task<List<Item>> GetItemsByUserIdAsync(int userId)
        {
            return await _context.Items
                                 .Where(i => i.OwnerId == userId)
                                 .ToListAsync();
        }

        /// <summary>
        /// Obtiene un Item por su ID.
        /// </summary>
        /// <param name="itemId">El ID del Item.</param>
        /// <returns>El Item si existe, de lo contrario, null.</returns>
        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            return await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        }

        // ------------------------------ Métodos de Tags ------------------------------

        /// <summary>
        /// Busca un Tag por su nombre.
        /// </summary>
        /// <param name="tagName">El nombre del tag a buscar.</param>
        /// <returns>El Tag si existe, de lo contrario, null.</returns>
        public async Task<Tag?> GetTagByNameAsync(string tagName)
        {
            return await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        }

        /// <summary>
        /// Añade un nuevo Tag a la base de datos.
        /// </summary>
        /// <param name="tag">El objeto Tag a añadir.</param>
        public async Task AddTagAsync(Tag tag)
        {
            await _context.Tags.AddAsync(tag);
            // IMPORTANTE: NO LLAMAMOS SaveChangesAsync AQUÍ.
            // La llamada a SaveChangesAsync se hará de forma explícita en el ItemService.
        }

        /// <summary>
        /// Añade múltiples ItemTag a la base de datos de forma eficiente.
        /// Este método coincide con la firma de IItemRepository.
        /// </summary>
        /// <param name="itemTags">Una colección de objetos ItemTag a añadir.</param>
        public async Task AddItemTagsAsync(List<ItemTag> itemTags) // <-- Coincide con la interfaz
        {
            await _context.ItemTags.AddRangeAsync(itemTags);
            // IMPORTANTE: NO LLAMAMOS SaveChangesAsync AQUÍ.
            // La llamada a SaveChangesAsync se hará de forma explícita en el ItemService.
        }

        // ------------------------------ Método de Persistencia (Unit of Work) ------------------------------

        /// <summary>
        /// Guarda todos los cambios pendientes en el contexto de la base de datos.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Post>> GetPostsByImageTagNamesAsync(IEnumerable<string> tagNames)
        {
            if (tagNames == null || !tagNames.Any())
            {
                return Enumerable.Empty<Post>();
            }

            var normalizedTagNames = tagNames.Select(t => t.ToLower()).Distinct().ToList();

            // La consulta ahora empieza desde la tabla de Posts
            var posts = await _context.Posts
                // Incluimos el Item que está asociado a este Post
                .Include(p => p.Item)
                    // Dentro del Item, incluimos sus Tags (y el objeto Tag real)
                    .ThenInclude(i => i.ItemTags)
                        .ThenInclude(it => it.Tag)
                // Incluimos otras propiedades del Item que necesitamos para el DTO
               // .Include(p => p.Item.ImageUrl) // Para la MainImageUrl
                .Include(p => p.Item.Category) // Para CategoryName
                .Include(p => p.Item.RentalType) // Para RentalTypeName
                .Include(p => p.Item.ItemStatus) // Para Status
                .Include(p => p.Item.Owner) // El Owner del Item (es un User)

                // Incluimos el Usuario que creó el Post
                .Include(p => p.User) // Para UserName del creador del Post

                // La condición de búsqueda: queremos Posts cuyo Item asociado tenga al menos una de las tags
                .Where(post => post.Item != null && // Nos aseguramos de que el Item asociado exista
                                post.Item.ItemTags.Any(itemTag =>
                                    normalizedTagNames.Contains(itemTag.Tag.Name.ToLower())
                                ))
                .ToListAsync();

            return posts;
        }
    }
}