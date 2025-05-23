using RentaloYa.Application.Common.DTOs;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IAuthService
    {
       Task<SupabaseAuthResultDto> VerifyTokenAndGetUserInfoAsync(string accessToken);
    }
}
