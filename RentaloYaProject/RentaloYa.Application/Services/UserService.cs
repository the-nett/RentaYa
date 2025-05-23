using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Domain.Entities;

namespace RentaloYa.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<RegistrationResult> RegisterUserFromExternalProviderAsync(string email, Guid? supabaseId, string fullName /*, Otros datos */)
        {
            try
            {
                // Verifica si el usuario ya existe
                var existingUser = await _userRepository.GetUserByUserNameAsync(email);
                if (existingUser = true)
                {
                    return new RegistrationResult { IsSuccess = false, ErrorMessage = "El nombre de usuario ya existe." };
                }
                else {
                    var newUser = new User
                    {
                        IdSupa = supabaseId,
                        Username = email, // Puedes generar un username único aquí si lo prefieres
                        FullName = fullName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        Gender_Id = 1, // O establecer un valor por defecto o solicitarlo en el registro adicional
                        //AuthProvider = "google" // O "supabase"
                        // Asigna aquí otros datos si los tienes disponibles
                    };
                    await _userRepository.AddUserAsync(newUser);
                    return new RegistrationResult { IsSuccess = true };
                }
            }
            catch (Exception ex)
            {
                return new RegistrationResult { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}
