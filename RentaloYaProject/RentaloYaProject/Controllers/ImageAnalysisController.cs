using Microsoft.AspNetCore.Mvc;
using RentaloYa.Application.Services.InterfacesServices;

namespace RentalWeb.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageAnalysisController : ControllerBase
    {
        private readonly IImageTaggingService _imageTaggingService; // ¡Ahora inyectamos la nueva interfaz!

        public ImageAnalysisController(IImageTaggingService imageTaggingService)
        {
            _imageTaggingService = imageTaggingService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeImage()
        {
            // 1. Verificar si se subió un archivo
            if (Request.Form.Files.Count == 0 || Request.Form.Files[0] == null)
            {
                return BadRequest("No se ha subido ningún archivo de imagen.");
            }

            var imageFile = Request.Form.Files[0]; // Obtén el primer archivo subido

            if (imageFile.Length == 0)
            {
                return BadRequest("El archivo de imagen está vacío.");
            }

            // 2. Leer la imagen a un byte array
            byte[] imageData;
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                imageData = memoryStream.ToArray();
            }

            // 3. Llamar al servicio de etiquetado de imágenes
            try
            {
                var tags = await _imageTaggingService.GetTagsFromImageAsync(imageData);
                return Ok(tags); // Devuelve las etiquetas como JSON
            }
            catch (HttpRequestException ex) // Errores de comunicación con la API externa
            {
                return StatusCode(500, $"Error al conectar con el servicio de IA: {ex.Message}");
            }
            catch (InvalidDataException ex) // Errores al parsear la respuesta del IA (ej. si el JSON no es el esperado)
            {
                return StatusCode(500, $"Error al procesar la respuesta de IA: {ex.Message}");
            }
            catch (Exception ex) // Otros errores inesperados
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
