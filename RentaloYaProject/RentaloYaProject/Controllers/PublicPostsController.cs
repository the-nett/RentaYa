using Microsoft.AspNetCore.Mvc;
using RentaloYa.Application.Services.InterfacesServices;
using RentalWeb.Web.ViewModels.Post;

namespace RentalWeb.Web.Controllers
{
    public class PublicPostsController : Controller
    {
        private readonly IPostService _postService;

        public PublicPostsController(IPostService postService)
        {
            _postService = postService;
        }
        // GET: /PublicPosts/Index
        public async Task<IActionResult> Index(string? searchTerm)
        {
            ViewBag.CurrentSearchTerm = searchTerm;
            var postsDto = await _postService.GetAllActivePostsForListingAsync(searchTerm);

            // Mapear de DTO a ViewModel si es necesario, o directamente usar el DTO si PostViewModel es igual a PostWithItemDto
            // Asumo que PostViewModel es un DTO que ya tiene todas las propiedades necesarias para la vista.
            // Si PostViewModel es idéntico a PostWithItemDto, podrías pasarlo directamente,
            // de lo contrario, haz el mapeo aquí.

            // Ejemplo de mapeo básico si PostViewModel es diferente:
            var posts = postsDto.Select(p => new PostViewModel
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                ItemName = p.ItemName,
                Status = p.Status,
                QuantityAvailable = p.QuantityAvailable,
                RentalType = p.RentalType,
                Price = p.Price,
                Location = p.Location,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UserName = p.Username
            }).ToList();


            return View(posts);
        }
    }
}
