using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talabat.Core.Entities.Identity;

namespace Talabat.Infrastructure._Identity.Config
{
    internal class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasOne(R => R.ApplicationUser).WithMany(A => A.RefreshTokens).HasForeignKey(R => R.ApplicationUserId);
            builder.HasIndex(R => R.Token).IsUnique();
        }
    }
}
