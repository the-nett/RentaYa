namespace RentalWeb.Web.ViewModels.Garage
{
    public class GarageItemViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string TipoRenta { get; set; } // "Día", "Semana", "Mes"
        public bool Disponible { get; set; }
        public string ImageUrl { get; set; }
    }
}
