using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;

namespace RentaloYa.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Include(p => p.Item)
                .Include(p => p.Item.RentalType) // si necesitas el tipo de alquiler
                .Include(p => p.Item.ItemStatus) // si necesitas el estado textual
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
        // Nuevo método para añadir un post
        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
        }

        // Nuevo método para verificar si ya existe un post para un ItemId
        public async Task<bool> PostExistsByItemIdAsync(int itemId)
        {
            return await _context.Posts.AnyAsync(p => p.ItemId == itemId);
        }
        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.Item)
                .Include(p => p.Item.RentalType)
                .Include(p => p.Item.ItemStatus)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        // Nuevo método para actualizar un post
        public Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            // No llamamos SaveChangesAsync() aquí, lo haremos en el servicio para mayor control transaccional si fuera necesario.
            return Task.CompletedTask;
        }

        // Método para guardar los cambios
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
        }
    }
}
