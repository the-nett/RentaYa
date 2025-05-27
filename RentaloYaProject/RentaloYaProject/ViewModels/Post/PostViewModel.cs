namespace RentalWeb.Web.ViewModels.Post
{
    public class PostViewModel
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
        public string RentalType { get; set; } = null!;
        public int QuantityAvailable { get; set; }
        public string UserName { get; set; } = null!; // Agregado para mostrar el nombre de usuario del creador del post
    }
}