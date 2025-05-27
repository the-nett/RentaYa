using RentaloYa.Application.Common.DTOs;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IPostService
    {
        Task<List<PostWithItemDto>> GetPostsByUserEmailAsync(string email);
    }
}
