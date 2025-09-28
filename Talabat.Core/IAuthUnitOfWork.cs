using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Talabat.Core
{
    public interface IAuthUnitOfWork:IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
