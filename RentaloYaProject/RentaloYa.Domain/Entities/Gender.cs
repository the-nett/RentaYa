using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RentaloYa.Domain.Entities
{
    public class Gender
    {
        [Key]
        public int IdGender { get; set; }
        public required string GenderName { get; set; } = string.Empty;
        public ICollection<User> users { get; set; } = new List<User>();

    }
}
