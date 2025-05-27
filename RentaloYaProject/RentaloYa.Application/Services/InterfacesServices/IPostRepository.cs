using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsByUserIdAsync(int userId);
    }
}
