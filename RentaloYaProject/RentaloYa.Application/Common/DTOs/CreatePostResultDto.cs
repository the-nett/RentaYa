namespace RentaloYa.Application.Common.DTOs
{
    public class CreatePostResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int? PostId { get; set; } // Opcional: el ID del post creado si la operación fue exitosa
    }
}
