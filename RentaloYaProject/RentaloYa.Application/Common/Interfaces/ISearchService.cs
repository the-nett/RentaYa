using RentaloYa.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.Interfaces
{
    public interface ISearchService
    {
        Task<IEnumerable<ItemSearchResultDto>> SearchItemsByImageAsync(byte[] imageData);
    }
}
