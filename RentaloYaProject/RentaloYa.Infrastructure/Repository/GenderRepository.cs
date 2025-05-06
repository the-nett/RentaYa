using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;

namespace RentaloYa.Infrastructure.Repository
{
    public class GenderRepository : IGenderRepository
    {
        private readonly ApplicationDbContext _context;
        public GenderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gender>> GetAllGenders()
        {
            return await _context.Genders.ToListAsync();
        }

    }
}
