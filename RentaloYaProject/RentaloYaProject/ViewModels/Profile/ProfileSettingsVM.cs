using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentaloYa.Web.ViewModels.Profile
{
    public class ProfileSettingsVM
    {
        public required string Username { get; set; } // Unique username
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public DateOnly Birthdate { get; set; }
        public required int Gender { get; set; }
    }
}
