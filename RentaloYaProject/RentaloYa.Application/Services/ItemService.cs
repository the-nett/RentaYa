using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;

namespace RentaloYa.Application.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<List<DetailedItemDto>> GetItemsByUserEmailAsync(string email)
        {
            var items = await _itemRepository.GetItemsByUserEmailAsync(email);

            return items.Select(item => new DetailedItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                CategoryName = item.Category.Name,
                RentalTypeName = item.RentalType.TypeName,
                Status = item.ItemStatus.StatusName
            }).ToList();
        }

    }

}
