using Microsoft.AspNetCore.Mvc;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Common.DTOs; // Importamos el DTO de nuestra capa de aplicación
using RentalWeb.Web.ViewModels.Post; // Importamos el ViewModel de nuestra capa web
using System.Collections.Generic;
using System.IO;
using System.Linq; // Necesario para .Select()
using System.Threading.Tasks;

namespace RentalWeb.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost("image")]
        public async Task<ActionResult<IEnumerable<ViewModels.Post.PostViewModel>>> SearchByImage([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageData = memoryStream.ToArray();

            // El servicio devuelve ItemSearchResultDto
            var itemSearchResults = await _searchService.SearchItemsByImageAsync(imageData);

            // AHORA: Mapeamos los ItemSearchResultDto a PostViewModel
            var postViewModels = itemSearchResults.Select(itemDto => new ViewModels.Post.PostViewModel
            {
                PostId = 0, // No tenemos un PostId directo aquí, podrías dejarlo en 0 o buscarlo si es posible
                            // Si el ItemSearchResultDto tuviera el PostId, lo usarías aquí.
                            // Por ahora, lo dejamos en 0 o podrías considerar si es necesario en la búsqueda.
                Title = itemDto.Name, // Usamos el nombre del Item como título del Post
                ImageUrl = itemDto.MainImageUrl,
                ItemName = itemDto.Name,
                Description = itemDto.Description,
                Status = itemDto.Status,
                CreatedAt = itemDto.CreatedAt,
                Location = itemDto.Location,
                Price = itemDto.Price,
                RentalType = itemDto.RentalTypeName,
                QuantityAvailable = itemDto.QuantityAvailable,
                UserName = itemDto.UserName
            }).ToList();

            return Ok(postViewModels);
        }
    }
}