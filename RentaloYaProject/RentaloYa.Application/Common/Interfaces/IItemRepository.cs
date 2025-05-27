using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IItemRepository
    {
        Task AddAsync(Item item);
        Task<List<Item>> GetItemsByUserEmailAsync(string email);
        Task<List<Item>> GetItemsByUserIdAsync(int userId);
        
        Task<Item?> GetItemByIdAsync(int itemId);
        Task<Tag?> GetTagByNameAsync(string tagName); // Nuevo: buscar un Tag por nombre
        Task AddTagAsync(Tag tag); // Nuevo: añadir un nuevo Tag
        Task AddItemTagsAsync(List<ItemTag> itemTags); // Nuevo: añadir relaciones ItemTag

        // SaveChangesAsync() ya estaría en IRepository<T> si lo manejas ahí
        // Si no lo manejas en IRepository<T>, añádelo aquí:
        Task SaveChangesAsync();
        //Task<IEnumerable<object>> GetItemsByTagNamesAsync(List<string> geminiTagNames);
        Task<IEnumerable<Post>> GetPostsByImageTagNamesAsync(IEnumerable<string> tagNames);
    }
}
