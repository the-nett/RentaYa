namespace RentaloYa.Application.Services.ModelsServices
{
    public class SupabaseAuthResult
    {
        public required bool IsAuthenticated { get; set; }
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; } // Asegúrate de que FullName esté en el metadata del usuario
        // Otros datos relevantes que Supabase pueda devolver
    }
}
