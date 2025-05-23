using RentaloYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email); // Método clave para la validación
        Task<RegistrationResult> RegisterUserFromExternalProviderAsync(string email, Guid? supabaseId, string fullName /*, Otros datos */);
    }

    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
