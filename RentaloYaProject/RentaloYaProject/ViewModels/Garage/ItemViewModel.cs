namespace RentalWeb.Web.ViewModels.Garage
{
    public class ItemViewModel
    {
        public int Id { get; set; } // ID del ítem
        public string Name { get; set; } // Nombre del ítem
        public string Description { get; set; } // Descripción breve
        public string ImageUrl { get; set; } // URL de la imagen del ítem (relativa a wwwroot)
        public decimal PricePerDay { get; set; } // Precio de renta por día
        public string Category { get; set; } // Categoría del ítem
    }
}
