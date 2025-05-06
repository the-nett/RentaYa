using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Domain.Entities
{
    public class RolePermission
    {
        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
