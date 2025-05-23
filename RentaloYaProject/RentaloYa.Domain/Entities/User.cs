using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RentaloYa.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Primary Key

        public Guid? IdSupa { get; set; } // UUID for Supabase

        public string? UsernameProsody { get; set; }
        public required string Username { get; set; } // Unique username    

        public required string FullName { get; set; }

        public required string Email { get; set; }

        public DateOnly Birthdate { get; set; }

        [ForeignKey("Gender")] // Navigation property will be named Gender
        public required int Gender_Id { get; set; }

        public DateTime? LastLogin { get; set; } // Nullable timestamp

        public required DateTime CreatedAt { get; set; }

        public required bool IsActive { get; set; }

        // Navigation property to the Gender entity
        public virtual Gender Gender { get; set; } = null!; // Initialize to null to avoid null reference issues
        public ICollection<UserRol> UserRoles { get; set; } = null!;

        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
