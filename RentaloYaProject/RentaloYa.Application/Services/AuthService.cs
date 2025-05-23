using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;

namespace RentaloYa.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ISupabaseAuthProvider _supabaseAuthProvider;

        public AuthService(ISupabaseAuthProvider supabaseAuthProvider)
        {
            _supabaseAuthProvider = supabaseAuthProvider;
        }

        public async Task<SupabaseAuthResultDto> VerifyTokenAndGetUserInfoAsync(string accessToken)
        {
            // Aquí iría cualquier lógica de negocio adicional relacionada con la verificación.
            // Por ahora, simplemente delegamos la llamada al proveedor de Supabase.
            return await _supabaseAuthProvider.GetUserInfoByTokenAsync(accessToken);
        }
    }
}
