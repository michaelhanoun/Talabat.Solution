using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Core.Specifications
{
    public class RefreshTokenSpecifications :BaseSpecifications<RefreshToken>
    {
        public RefreshTokenSpecifications(string token):base(R=>R.Token==token)
        {
            
        }
    }
}
