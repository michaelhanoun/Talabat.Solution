using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
    public class DeliveryMethod :BaseEntity
    {
        public required string ShortName { get; set; }
        public required string Description { get; set; }
        public  decimal Cost { get; set; }
        public required string DeliveryTime { get; set; }
    }
}
