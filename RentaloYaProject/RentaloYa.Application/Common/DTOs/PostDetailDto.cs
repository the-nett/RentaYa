namespace RentaloYa.Application.Common.DTOs
{
    public class PostDetailDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int ItemId { get; set; }
        public string? CurrentItemName { get; set; } // Nombre del artículo actual (para mostrar)

        public List<ItemDto> UserItems { get; set; } = new List<ItemDto>(); // Lista de ítems del usuario
    }
}
