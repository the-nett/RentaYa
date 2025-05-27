using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;

namespace RentaloYa.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public PostService(IPostRepository postRepository, IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<List<PostWithItemDto>> GetPostsByUserEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return new List<PostWithItemDto>();

            var posts = await _postRepository.GetPostsByUserIdAsync(user.Id);

            return posts.Select(p => new PostWithItemDto
            {
                PostId = p.PostId,
                Title = p.Title,
                CreatedAt = p.CreatedAt,
                ItemName = p.Item?.Name ?? "(Sin nombre)",
                Description = p.Description ?? "(Sin descripción)",
                ImageUrl = p.Item?.ImageUrl,
                Status = p.Item?.ItemStatus?.StatusName ?? "Desconocido",
                Location = p.Item.Location,
                Price = p.Item.Price,
                RentalType = p.Item.RentalType.TypeName, // Fix: Access the Id property of RentalType
                QuantityAvailable = p.Item.QuantityAvailable
            }).ToList();
        }
    }
}
