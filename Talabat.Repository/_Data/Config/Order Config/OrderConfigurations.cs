using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Infrastructure._Data.Config.Order_Config
{
    internal class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(order => order.ShippingAddress, ShippingAddress => ShippingAddress.WithOwner());
            builder.Property(order => order.Status).HasConversion(O => O.ToString(), o => (OrderStatus)Enum.Parse(typeof(OrderStatus),o));
            builder.Property(order => order.Subtotal).HasColumnType("decimal(12,2)");
            builder.HasOne(O=>O.DeliveryMethod).WithMany().OnDelete(DeleteBehavior.SetNull);
            builder.HasMany(O=>O.Items).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
