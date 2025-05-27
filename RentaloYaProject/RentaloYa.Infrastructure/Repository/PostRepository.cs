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
    }
}
