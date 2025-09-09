using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Entities.Product;
using Talabat.Core.Repositories.Contract;
using Talabat.Infrastructure.Data;
using Talabat.Infrastructure.Generic_Repository;

namespace Talabat.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _storeContext;

        private Hashtable _repositories;
        public UnitOfWork(StoreContext storeContext)
        {
            _storeContext = storeContext;
            _repositories = new Hashtable();
        }
        public async Task<int> CompleteAsync()
        {
            return await _storeContext.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
           await _storeContext.DisposeAsync();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var key = typeof(TEntity).Name;
            if(!_repositories.ContainsKey(key))
            {
               var repo = new GenericRepository<TEntity>(_storeContext);
                _repositories.Add(key,repo);
            }
            return _repositories[key] as IGenericRepository<TEntity>;
        }
    }
}
