namespace RentaloYa.Application.Common.DTOs
{
    public class PostWithItemDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ItemName { get; set; }
        public string Description { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Location { get; set; }
        public decimal Price { get; set; }
        public string RentalType { get; set; }
        public int QuantityAvailable { get; set; }

    }
}
