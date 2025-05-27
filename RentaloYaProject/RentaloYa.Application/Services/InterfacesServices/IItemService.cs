using RentaloYa.Application.Common.DTOs;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IItemService
    {
        Task<List<DetailedItemDto>> GetItemsByUserEmailAsync(string email);
        Task<(bool Success, string? ErrorMessage, int? CreatedItemId)> CreateItemWithTagsAsync(
            CreateItemDto itemDto, // <-- Ahora recibe el CreateItemDto
            string userEmail,
            Func<byte[], Task<string?>> uploadImageFunc);
    }
}
