using RentaloYa.Application.Common.DTOs;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IPostService
    {
        Task<List<PostWithItemDto>> GetPostsByUserEmailAsync(string email);
        Task<CreatePostResultDto> CreatePostAsync(CreatePostDto createPostDto);
        Task<List<ItemDto>> GetUserItemsForPostCreationAsync(string userEmail); // Para la creación, aún útil para la edición
        Task<PostDetailDto?> GetPostForEditAsync(int postId, string userEmail); // Nuevo método para cargar datos para edición
        Task<EditPostResultDto> EditPostAsync(EditPostDto editPostDto);
        Task<DeletePostResultDto> DeletePostAsync(int postId, string userEmail);
    }
}
