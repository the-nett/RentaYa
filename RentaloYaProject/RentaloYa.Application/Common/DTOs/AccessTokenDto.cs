using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Application.Common.DTOs
{
    // En RentalWeb.Web/ViewModels (o donde prefieras tus DTOs)
    public class AccessTokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresAt { get; set; }
        public string TokenType { get; set; }
        public string ProviderToken { get; set; }
        public string ProviderRefreshToken { get; set; }

    }
}
