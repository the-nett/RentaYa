using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Domain.Entities
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }
        public required string Permissionn { get; set; }
        public string? Description { get; set; }

        public ICollection<RolePermission> RolesPermissions { get; set; } = new List<RolePermission>();
    }
}
