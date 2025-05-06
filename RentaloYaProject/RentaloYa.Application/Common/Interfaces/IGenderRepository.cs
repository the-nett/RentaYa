using RentaloYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface IGenderRepository
    {
        Task<IEnumerable<Gender>> GetAllGenders();
    }
}
