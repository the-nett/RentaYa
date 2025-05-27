namespace RentaloYa.Application.Common.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public int TipoRenta { get; set; }
        public bool Disponible { get; set; }
    }
}
