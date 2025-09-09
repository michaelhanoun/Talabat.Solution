using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Infrastructure.Data;

namespace Talabat.Infrastructure.Generic_Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext _storeContext;
        public GenericRepository(StoreContext storeContext)
        {
            _storeContext = storeContext; 
        }

        public async Task Add(T entity)
        {
            await _storeContext.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _storeContext.Remove(entity);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            //if(typeof(T) == typeof(Product))
            //return (IEnumerable<T>) await _storeContext.Set<Product>().Include( P => P.Brand ).Include(P=>P.Category).AsNoTracking().ToListAsync();
            return await _storeContext.Set<T>().AsNoTracking().ToListAsync();

        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            //if (typeof(T) == typeof(Product))
            //    return await _storeContext.Set<Product>().Include(P => P.Brand).Include(P => P.Category).AsNoTracking().FirstOrDefaultAsync(P => P.Id == id) as T;
            return await _storeContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(P => P.Id == id);
        }
        public async Task<T?> GetByIdWithTrackingAsync(int id)
        {
            //if (typeof(T) == typeof(Product))
            //    return await _storeContext.Set<Product>().Include(P => P.Brand).Include(P => P.Category).AsNoTracking().FirstOrDefaultAsync(P => P.Id == id) as T;
            return await _storeContext.Set<T>().FirstOrDefaultAsync(P => P.Id == id);
        }

        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).CountAsync();
        }

        public async Task<T?> GetWithSpecAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).AsNoTracking().FirstOrDefaultAsync();

        }

        public void Update(T entity)
        {
            _storeContext.Update(entity);
        }

        private IQueryable<T> ApplySpecifications(ISpecifications<T> specifications)
        {
            return SpecificationsEvaluator<T>.GetQuery(_storeContext.Set<T>(), specifications);
        }
    }
}
 