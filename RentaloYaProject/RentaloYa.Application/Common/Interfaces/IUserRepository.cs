using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> GetUserByUserNameAsync(string userName);
    }
}
