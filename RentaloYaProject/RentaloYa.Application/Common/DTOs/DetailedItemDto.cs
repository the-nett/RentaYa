namespace RentaloYa.Application.Common.DTOs
{
    public class DetailedItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public string RentalTypeName { get; set; }
        public string Status { get; set; }
    }
}
