using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Common.DTOs;
using RentaloYa.Domain.Entities; // Para Post, Item, User, etc.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentaloYa.Application.Services.InterfacesServices;

namespace RentaloYa.Application.Services
{
    public class SearchService : ISearchService
    {
        private readonly IImageTaggingService _imageTaggingService;
        private readonly IItemRepository _itemRepository;

        public SearchService(IImageTaggingService imageTaggingService, IItemRepository itemRepository)
        {
            _imageTaggingService = imageTaggingService;
            _itemRepository = itemRepository;
        }

        public async Task<IEnumerable<ItemSearchResultDto>> SearchItemsByImageAsync(byte[] imageData)
        {
            var geminiTaggingResults = await _imageTaggingService.GetTagsFromImageAsync(imageData);

            var geminiTagNames = geminiTaggingResults
                                    .Select(tr => tr.Name.ToLower())
                                    .Distinct()
                                    .ToList();

            if (!geminiTagNames.Any())
            {
                return Enumerable.Empty<ItemSearchResultDto>();
            }

            var matchedPosts = await _itemRepository.GetPostsByImageTagNamesAsync(geminiTagNames);

            var results = new List<ItemSearchResultDto>();
            foreach (var post in matchedPosts)
            {
                var item = post.Item;
                var user = post.User;

                if (item != null && user != null)
                {
                    results.Add(new ItemSearchResultDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        // ¡CORRECCIÓN AQUÍ! Usamos item.ImageUrl directamente, no item.Images
                        MainImageUrl = item.ImageUrl,
                        CategoryName = item.Category?.Name,
                        RentalTypeName = item.RentalType?.TypeName,
                        UserName = user.Username,
                        Status = item.ItemStatus?.StatusName,
                        QuantityAvailable = item.QuantityAvailable,
                        CreatedAt = post.CreatedAt,
                        // ¡CORRECCIÓN AQUÍ! Location es una propiedad de Item, no de Post.
                        Location = item.Location
                    });
                }
            }

            return results;
        }
    }
}