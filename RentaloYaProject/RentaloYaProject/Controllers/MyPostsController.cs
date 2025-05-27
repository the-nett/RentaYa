using Microsoft.AspNetCore.Mvc;
using RentaloYa.Application.Services.InterfacesServices;
using RentalWeb.Web.ViewModels.Post;
using System.Security.Claims;

namespace RentalWeb.Web.Controllers
{
    public class MyPostsController : Controller
    {
        private readonly IPostService _postService;

        public MyPostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var postDtos = await _postService.GetPostsByUserEmailAsync(email);

            var viewModel = postDtos.Select(p => new PostViewModel
            {
                PostId = p.PostId,
                Title = p.Title,
                CreatedAt = p.CreatedAt,
                ItemName = p.ItemName,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                Location =p.Location,
                Price = p.Price,
                RentalType = p.RentalType,
                QuantityAvailable = p.QuantityAvailable

            }).ToList();

            return View(viewModel);
        }
    }
}
