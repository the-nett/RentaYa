using Microsoft.Extensions.Options;
using RentaloYa.Infrastructure.Configuration;
using Supabase.Gotrue;

namespace RentaloYa.Infrastructure.ExternalServices
{
    public class SupabaseAuthService
    {
        private readonly SupabaseSettings _supabaseSettings;

        public SupabaseAuthService(IOptions<SupabaseSettings> supabaseSettings)
        {
            _supabaseSettings = supabaseSettings.Value;
        }

        public void InitializeSupabase()
        {
            var auth = new Client(new ClientOptions
            {
                Url = _supabaseSettings.Url,
                Headers = new Dictionary<string, string>
            {
                { "apikey", _supabaseSettings.PublicKey }
            }
            });

            // Tu lógica aquí
        }
    }
}