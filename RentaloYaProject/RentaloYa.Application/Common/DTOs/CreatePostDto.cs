namespace RentaloYa.Application.Common.DTOs
{
    public class CreatePostDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ItemId { get; set; } // El ID del artículo al que se asocia este post
        public int UserId { get; set; } // El ID del usuario que crea el post
    }
}
