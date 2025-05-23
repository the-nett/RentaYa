using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;

namespace RentaloYa.Infrastructure.Repository
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;
        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Item>> GetItemsByUserEmailAsync(string email)
        {
            return await _context.Items
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.RentalType)
                .Include(i => i.ItemStatus)
                .Where(i => i.Owner.Email == email /* && !i.IsDeleted */) // descomenta si manejas soft delete
                .ToListAsync();
        }
    }
}
