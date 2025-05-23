using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Services.ModelsServices;
using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface ISupabaseAuthProvider
    {
        Task<SupabaseAuthResultDto> GetUserInfoByTokenAsync(string accessToken);
    }
}
