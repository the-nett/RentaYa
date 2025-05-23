using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IItemRepository
    {
        Task<List<Item>> GetItemsByUserEmailAsync(string email);
    }
}
