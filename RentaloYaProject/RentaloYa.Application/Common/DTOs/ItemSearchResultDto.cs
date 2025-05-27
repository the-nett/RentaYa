using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.DTOs
{
    public class ItemSearchResultDto
    {
        public int Id { get; set; } // ID del Item
        public string Name { get; set; } // Nombre del Item
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string MainImageUrl { get; set; } // URL de la imagen principal del Item
        public string CategoryName { get; set; } // Nombre de la categoría asociada al Item
        public string RentalTypeName { get; set; } // Nombre del tipo de alquiler asociado al Item
        public string UserName { get; set; } // Nombre del usuario que publicó el Item/Post
        public string Status { get; set; } // Estado del Item
        public int QuantityAvailable { get; set; } // Cantidad disponible del Item
        public DateTime CreatedAt { get; set; } // Fecha de creación del Post asociado al Item (necesitarás buscar esto)
        public string Location { get; set; } // Ubicación del Post/Item
    }
}
