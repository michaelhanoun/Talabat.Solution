using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Models
{
    public class AuthResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpireOfRefreshToken { get; set; }
    }
}
