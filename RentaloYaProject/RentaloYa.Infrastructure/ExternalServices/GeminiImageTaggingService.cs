using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentaloYa.Application.Services.InterfacesServices;
using System.Text.Json;
using System.Text;

namespace RentaloYa.Infrastructure.ExternalServices
{
    public class GeminiImageTaggingService : IImageTaggingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelId;
        private readonly string _prompt;
        private readonly ILogger<GeminiImageTaggingService> _logger;

        public GeminiImageTaggingService(HttpClient httpClient, IConfiguration config, ILogger<GeminiImageTaggingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Lee la configuración de appsettings.json
            _apiKey = config["GeminiVision:ApiKey"] ??
                      throw new InvalidOperationException("GeminiVision:ApiKey no configurado en appsettings.");
            _modelId = config["GeminiVision:ModelId"] ??
                       throw new InvalidOperationException("GeminiVision:ModelId no configurado en appsettings.");
            _prompt = config["GeminiVision:Prompt"] ??
                      throw new InvalidOperationException("GeminiVision:Prompt no configurado en appsettings.");

            // Configura la URL base para Gemini.
            // La API de Gemini Pro Vision suele usar esta base:
            // "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent"
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        }

        public async Task<IEnumerable<ImageTaggingResult>> GetTagsFromImageAsync(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("Se intentó analizar una imagen vacía o nula con Gemini. No se procesará.");
                return new List<ImageTaggingResult>();
            }

            _logger.LogInformation("Enviando imagen para análisis a Google Gemini Vision.");

            try
            {
                // Codifica la imagen a Base64 para enviarla en el JSON de la solicitud
                string base64Image = Convert.ToBase64String(imageData);

                // Construye el cuerpo de la solicitud JSON para Gemini
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = _prompt }, // Nuestro prompt de texto
                                new {
                                    inlineData = new
                                    {
                                        mimeType = "image/jpeg", // O "image/png" si es el caso
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = false });
                using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // La URL completa del endpoint de Gemini incluye el ID del modelo y la API Key
                string requestUri = $"v1beta/models/{_modelId}:generateContent?key={_apiKey}";

                // Envía la solicitud POST a Gemini
                var response = await _httpClient.PostAsync(requestUri, content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error del servicio Gemini: Código de estado {response.StatusCode}. Detalles: {responseContent}");
                    throw new HttpRequestException($"La solicitud a Google Gemini falló con el código de estado {response.StatusCode}. Detalles: {responseContent}");
                }

                _logger.LogDebug($"Respuesta exitosa de Gemini: {responseContent}");

                // --- Deserialización de la Respuesta de Gemini ---
                // Gemini devuelve el resultado en 'candidates[0].content.parts[0].text'
                // Y esperamos que ese texto sea un JSON con nuestra estructura
                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var textContent = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(textContent))
                {
                    _logger.LogWarning("La respuesta de Gemini no contenía el texto esperado para el análisis.");
                    return new List<ImageTaggingResult>();
                }
                if (textContent.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    // Encuentra la primera aparición de '{' para saber dónde empieza el JSON real
                    int jsonStartIndex = textContent.IndexOf('{');
                    if (jsonStartIndex != -1)
                    {
                        textContent = textContent.Substring(jsonStartIndex);
                    }
                }

                // Intenta remover el final del bloque de código
                if (textContent.EndsWith("```", StringComparison.OrdinalIgnoreCase))
                {
                    // Encuentra la última aparición de '}' para saber dónde termina el JSON real
                    int jsonEndIndex = textContent.LastIndexOf('}');
                    if (jsonEndIndex != -1)
                    {
                        // Sumamos 1 para incluir el '}'
                        textContent = textContent.Substring(0, jsonEndIndex + 1);
                    }
                }

                textContent = textContent.Trim();

                // Intentar parsear el JSON que Gemini debería haber generado
                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ImageTaggingRootResponse>(textContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return apiResponse?.TagsResult?.Values?.Select(tag => new ImageTaggingResult
                    {
                        Name = tag.Name,
                        Confidence = (float)tag.Confidence
                    }) ?? new List<ImageTaggingResult>();
                }
                catch (JsonException parseEx)
                {
                    _logger.LogError(parseEx, $"Error al parsear el JSON generado por Gemini desde el texto: {textContent}");
                    throw new InvalidDataException("La respuesta de Gemini no pudo ser parseada como el JSON de tags esperado.", parseEx);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red o protocolo al comunicarse con Google Gemini.");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar la respuesta JSON de la API de Gemini (estructura principal).");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Un error inesperado ocurrió durante el análisis de la imagen con Google Gemini.");
                throw;
            }
        }

        // --- Clases Auxiliares para la Deserialización JSON de la API de Gemini ---
        // (Estas clases mapean la estructura real de la respuesta de la API de Gemini)
        private class GeminiApiResponse
        {
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            public Content? Content { get; set; }
            public string? FinishReason { get; set; }
            public int Index { get; set; }
        }

        private class Content
        {
            public List<Part>? Parts { get; set; }
            public string? Role { get; set; }
        }

        private class Part
        {
            public string? Text { get; set; }
        }

        // --- Clases Auxiliares para la Deserialización del JSON que esperamos de Gemini ---
        // (Estas son las clases que mapean el JSON que le pedimos a Gemini que genere en el prompt)
        private class ImageTaggingRootResponse
        {
            public TagsResultContainer? TagsResult { get; set; }
        }

        private class TagsResultContainer
        {
            public List<ImageTagValue>? Values { get; set; }
        }

        private class ImageTagValue
        {
            public string Name { get; set; } = null!;
            public double Confidence { get; set; } // Lo dejamos como double, luego se castea a float
        }
    }
}
