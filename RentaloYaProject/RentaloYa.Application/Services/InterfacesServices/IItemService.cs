using RentaloYa.Application.Common.DTOs;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IItemService
    {
        Task<List<DetailedItemDto>> GetItemsByUserEmailAsync(string email);
    }
}
