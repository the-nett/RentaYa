using Microsoft.Extensions.Configuration;
using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;


namespace RentaloYa.Infrastructure.ExternalServices
{
    public class SupabaseAuthClient : ISupabaseAuthProvider
    {
        private readonly Supabase.Client _supabaseClient;

        public SupabaseAuthClient(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<SupabaseAuthResultDto> GetUserInfoByTokenAsync(string accessToken)
        {
            try
            {
                var user = await _supabaseClient.Auth.GetUser(accessToken);
                var name = user.UserMetadata.ContainsKey("full_name")
                    ? user.UserMetadata["full_name"]?.ToString()
                    : null;

                if (user != null)
                {
                    return new SupabaseAuthResultDto
                    {
                        IsAuthenticated = true,
                        Id = user.Id,
                        Email = user.Email,
                        Name = name
                    };
                }
                else
                {
                    return new SupabaseAuthResultDto { IsAuthenticated = false };
                }

            }
            catch (Exception)
            {
                // Manejar cualquier error de la biblioteca de Supabase o de la red
                return new SupabaseAuthResultDto { IsAuthenticated = false };
            }
        }
    }
}