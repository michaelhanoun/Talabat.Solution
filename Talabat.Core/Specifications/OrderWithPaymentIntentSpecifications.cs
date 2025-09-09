using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;


namespace Talabat.Core.Specifications
{
    public class OrderWithPaymentIntentSpecifications:BaseSpecifications<Order>
    {
        public OrderWithPaymentIntentSpecifications(string paymentIntendId):base(O=>O.PaymentIntentId == paymentIntendId)
        {
            
        }
    }
}
