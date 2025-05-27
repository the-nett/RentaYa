namespace RentaloYa.Application.Common.DTOs
{
    public class EditPostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ItemId { get; set; }
        public int UserId { get; set; } // Para validación de propiedad
    }
}
