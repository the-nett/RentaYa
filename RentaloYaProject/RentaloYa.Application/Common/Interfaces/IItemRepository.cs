using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IItemRepository
    {
        Task<List<Item>> GetItemsByUserEmailAsync(string email);
        Task<List<Item>> GetItemsByUserIdAsync(int userId);
        
        Task<Item?> GetItemByIdAsync(int itemId);
    }
}
