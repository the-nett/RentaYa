using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Domain.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public required string Rol { get; set; }
        public string? Description { get; set; }

        public ICollection<UserRol> UsersRoles { get; set; } = new List<UserRol>();
        public ICollection<RolePermission> RolesPermissions { get; set; } = new List<RolePermission>();
    }
}
