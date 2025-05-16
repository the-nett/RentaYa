using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<User?> ValidateSessionAsync(string accessToken);
    }
}
