using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Domain.Entities;
using RentaloYa.Infrastructure.Data;

namespace RentaloYa.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return _context.SaveChangesAsync();
        }

         public async Task AddUserAsync(User user)
         {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
         }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task<bool> GetUserByUserNameAsync(string userName)
        {
            return await _context.Users.AnyAsync(x => x.Username == userName);
        }
    }
}