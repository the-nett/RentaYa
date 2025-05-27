using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.DTOs
{
    public class DeletePostResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }
}
