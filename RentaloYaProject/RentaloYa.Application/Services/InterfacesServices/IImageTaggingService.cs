namespace RentaloYa.Application.Services.InterfacesServices
{
    // Esta clase representa el resultado de una etiqueta generada por IA
    public class ImageTaggingResult
    {
        public string Name { get; set; } = null!;
        public float Confidence { get; set; } // Usamos 'float' para coincidir con tu entidad ItemTag.Confidence
    }

    // Esta es la interfaz que tu servicio de Azure Computer Vision implementará
    public interface IImageTaggingService
    {
        /// <summary>
        /// Obtiene etiquetas de una imagen enviada como un arreglo de bytes.
        /// </summary>
        /// <param name="imageData">Arreglo de bytes de la imagen a analizar.</param>
        /// <returns>Una colección de objetos ImageTaggingResult con el nombre de la etiqueta y su confianza.</returns>
        Task<IEnumerable<ImageTaggingResult>> GetTagsFromImageAsync(byte[] imageData);
    }
}
