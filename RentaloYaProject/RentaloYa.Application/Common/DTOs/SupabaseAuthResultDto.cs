namespace RentaloYa.Application.Common.DTOs
{
    public class SupabaseAuthResultDto
    {
        public bool IsAuthenticated { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; } // Asegúrate de que FullName esté en el metadata del usuario
        // Otros datos relevantes que Supabase pueda devolver
    }
}
