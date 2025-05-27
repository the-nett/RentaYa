using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentaloYa.Application.Services.InterfacesServices;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RentaloYa.Infrastructure.ExternalServices
{
    public class AzureImageTaggingService : IImageTaggingService // ¡Implementa la interfaz!
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _key;
        private readonly ILogger<AzureImageTaggingService> _logger;

        public AzureImageTaggingService(HttpClient httpClient, IConfiguration config, ILogger<AzureImageTaggingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            
            _endpoint = config["AzureVision:Endpoint"] ??
                        throw new InvalidOperationException("AzureVision:Endpoint no configurado en appsettings.");
            _key = config["AzureVision:Key"] ??
                    throw new InvalidOperationException("AzureVision:Key no configurado en appsettings.");

            // Si tienes un BaseAddress para HttpClient, podrías configurarlo aquí.
            // Por ejemplo: _httpClient.BaseAddress = new Uri("https://mvprentalo.cognitiveservices.azure.com/");
            // Y luego en el método usar solo el path: "/computervision/imageanalysis:analyze?api-version=2024-02-01&features=tags"
        }

        public async Task<IEnumerable<ImageTaggingResult>> GetTagsFromImageAsync(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("Se intentó analizar una imagen vacía o nula. No se procesará.");
                return new List<ImageTaggingResult>();
            }

            _logger.LogInformation("Enviando imagen para análisis a Azure Computer Vision.");

            try
            {
                // Prepara el contenido de la imagen como un arreglo de bytes
                using var content = new ByteArrayContent(imageData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream"); // Tipo de contenido correcto para imagen

                // Crea la solicitud HTTP
                using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key); // Añade la clave de suscripción
                request.Content = content;

                // Envía la solicitud
                var response = await _httpClient.SendAsync(request);

                // Lee el contenido de la respuesta, incluso si es un error, para un mejor diagnóstico
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error del servicio Azure CV: Código de estado {response.StatusCode}. Detalles: {responseContent}");
                    // Lanza una excepción para que el llamador pueda manejar el error
                    throw new HttpRequestException($"La solicitud a Azure Computer Vision falló con el código de estado {response.StatusCode}. Detalles: {responseContent}");
                }

                _logger.LogDebug($"Respuesta exitosa de Azure CV: {responseContent}");

                // Deserializa la respuesta JSON de Azure
                // Necesitas las clases auxiliares para mapear la estructura del JSON
                var apiResponse = JsonSerializer.Deserialize<AzureVisionResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Mapea los resultados al formato ImageTaggingResult
                return apiResponse?.TagsResult?.Values?.Select(tag => new ImageTaggingResult
                {
                    Name = tag.Name,
                    Confidence = (float)tag.Confidence // Convierte double a float
                }) ?? new List<ImageTaggingResult>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red o protocolo al comunicarse con Azure Computer Vision.");
                throw; // Re-lanza para que la capa superior maneje el error
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar la respuesta JSON de Azure Computer Vision. La respuesta puede no tener el formato esperado.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Un error inesperado ocurrió durante el análisis de la imagen con Azure Computer Vision.");
                throw;
            }
        }

        // --- Clases Auxiliares para la Deserialización JSON ---
        private class AzureVisionResponse
        {
            public TagsResultContainer? TagsResult { get; set; }
        }

        private class TagsResultContainer
        {
            public List<AzureTagValue>? Values { get; set; }
        }

        private class AzureTagValue
        {
            public string Name { get; set; } = null!;
            public double Confidence { get; set; } // El JSON de Azure devuelve la confianza como double
        }
    }
    
}
