using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Services.InterfacesServices
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsByUserIdAsync(int userId);
        Task AddPostAsync(Post post);
        Task<bool> PostExistsByItemIdAsync(int itemId);
        Task<Post?> GetPostByIdAsync(int postId); // Nuevo método para obtener un post por ID
        Task UpdatePostAsync(Post post); // Nuevo método para actualizar un post
        Task DeletePostAsync(int postId);
        Task SaveChangesAsync(); // Necesario si UpdateAsync no guarda los cambios internamente
    }
}
