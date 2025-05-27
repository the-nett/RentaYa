using RentaloYa.Application.Common.DTOs;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IItemRepository _itemRepository;

        public PostService(IPostRepository postRepository, IUserRepository userRepository, IItemRepository itemRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _itemRepository = itemRepository;
        }

        public async Task<List<PostWithItemDto>> GetPostsByUserEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return new List<PostWithItemDto>();

            var posts = await _postRepository.GetPostsByUserIdAsync(user.Id);

            return posts.Select(p => new PostWithItemDto
            {
                PostId = p.PostId,
                Title = p.Title,
                CreatedAt = p.CreatedAt,
                ItemName = p.Item?.Name ?? "(Sin nombre)",
                Description = p.Description ?? "(Sin descripción)",
                ImageUrl = p.Item?.ImageUrl,
                Status = p.Item?.ItemStatus?.StatusName ?? "Desconocido",
                Location = p.Item.Location,
                Price = p.Item.Price,
                RentalType = p.Item.RentalType.TypeName, // Fix: Access the Id property of RentalType
                QuantityAvailable = p.Item.QuantityAvailable
            }).ToList();
        }
        // Nuevo método para crear un post
        public async Task<CreatePostResultDto> CreatePostAsync(CreatePostDto createPostDto)
        {
            // 1. Validar si ya existe un post para el ItemId
            var postExists = await _postRepository.PostExistsByItemIdAsync(createPostDto.ItemId);
            if (postExists)
            {
                return new CreatePostResultDto
                {
                    Success = false,
                    Message = "Ya existe un post para este artículo. Solo se permite un post por artículo."
                };
            }

            // 2. Opcional: Validar si el ItemId existe y pertenece al UserId (si esta lógica debe estar en el servicio)
            // Esto dependerá de si un usuario solo puede crear posts para sus propios artículos.
            // Para este ejemplo, asumiremos que la relación entre UserId e ItemId es válida,
            // pero en un sistema real, querrías una validación más robusta aquí.
            // Puedes inyectar un IItemRepository y verificar item.UserId == createPostDto.UserId

            // 3. Crear la entidad Post a partir del DTO
            var newPost = new Post
            {
                Title = createPostDto.Title,
                Description = createPostDto.Description,
                ItemId = createPostDto.ItemId,
                UserId = createPostDto.UserId,
                CreatedAt = DateTime.UtcNow // Establece la fecha de creación
            };

            // 4. Guardar el nuevo post a través del repositorio
            await _postRepository.AddPostAsync(newPost);

            // 5. Devolver el resultado exitoso
            return new CreatePostResultDto
            {
                Success = true,
                Message = "Post creado exitosamente.",
                PostId = newPost.PostId
            };
        }
        public async Task<List<ItemDto>> GetUserItemsForPostCreationAsync(string userEmail)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return new List<ItemDto>();
            }

            var items = await _itemRepository.GetItemsByUserIdAsync(user.Id);

            return items.Select(i => new ItemDto
            {
                Id = i.Id,
                Nombre = i.Name
            }).ToList();
        }
        // Implementación corregida para devolver PostDetailDto
        public async Task<PostDetailDto?> GetPostForEditAsync(int postId, string userEmail)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            if (user == null) return null;

            var post = await _postRepository.GetPostByIdAsync(postId);

            if (post == null || post.UserId != user.Id) return null;

            var userItems = await _itemRepository.GetItemsByUserIdAsync(user.Id);

            var userItemDtos = userItems.Select(item => new ItemDto
            {
                Id = item.Id,
                Nombre = item.Name
            }).ToList();

            return new PostDetailDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                ItemId = post.ItemId,
                CurrentItemName = post.Item?.Name,
                UserItems = userItemDtos // Ahora pasamos la lista de DTOs de ítems
            };
        }

        public async Task<EditPostResultDto> EditPostAsync(EditPostDto editPostDto)
        {
            var postToUpdate = await _postRepository.GetPostByIdAsync(editPostDto.PostId);

            // Validar que el post exista y pertenezca al usuario
            if (postToUpdate == null || postToUpdate.UserId != editPostDto.UserId)
            {
                return new EditPostResultDto
                {
                    Success = false,
                    Message = "Post no encontrado o no tienes permisos para editarlo."
                };
            }

            // Validar que el nuevo ItemId exista y pertenezca al usuario
            var newItem = await _itemRepository.GetItemByIdAsync(editPostDto.ItemId);
            if (newItem == null || newItem.OwnerId != editPostDto.UserId)
            {
                return new EditPostResultDto
                {
                    Success = false,
                    Message = "El artículo seleccionado no es válido o no pertenece a tu cuenta."
                };
            }

            // <--- INICIO DE LA CORRECCIÓN DE LA LÓGICA DE VALIDACIÓN --->
            // Solo valida "un post por artículo" si el ItemId HA CAMBIADO.
            // Si el ItemId es el mismo, entonces ya existe el post (este mismo post) para ese ItemId,
            // y no debe generar un error de duplicado.
            if (postToUpdate.ItemId != editPostDto.ItemId) // Si el ItemId ha sido modificado
            {
                var postExistsForNewItem = await _postRepository.PostExistsByItemIdAsync(editPostDto.ItemId);
                if (postExistsForNewItem)
                {
                    return new EditPostResultDto
                    {
                        Success = false,
                        Message = "Ya existe un post para el artículo seleccionado. Solo se permite un post por artículo."
                    };
                }
            }

            // Actualizar las propiedades del post
            postToUpdate.Title = editPostDto.Title;
            postToUpdate.Description = editPostDto.Description;
            postToUpdate.ItemId = editPostDto.ItemId;

            // Guardar los cambios
            await _postRepository.UpdatePostAsync(postToUpdate);
            await _postRepository.SaveChangesAsync();

            return new EditPostResultDto
            {
                Success = true,
                Message = "Post actualizado exitosamente."
            };

        }
        public async Task<DeletePostResultDto> DeletePostAsync(int postId, string userEmail)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return new DeletePostResultDto
                {
                    Success = false,
                    Message = "Usuario no autenticado."
                };
            }

            var postToDelete = await _postRepository.GetPostByIdAsync(postId);

            // Validar que el post exista y pertenezca al usuario
            if (postToDelete == null || postToDelete.UserId != user.Id)
            {
                return new DeletePostResultDto
                {
                    Success = false,
                    Message = "Post no encontrado o no tienes permisos para eliminarlo."
                };
            }

            // Eliminar el post a través del repositorio
            await _postRepository.DeletePostAsync(postId);
            await _postRepository.SaveChangesAsync(); // Asegurarse de guardar los cambios

            return new DeletePostResultDto
            {
                Success = true,
                Message = "Post eliminado exitosamente."
            };
        }
    }
}
