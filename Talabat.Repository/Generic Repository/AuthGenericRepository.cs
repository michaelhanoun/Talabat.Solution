using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Infrastructure._Identinty;
using Talabat.Infrastructure.Data;

namespace Talabat.Infrastructure.Generic_Repository
{
    public class AuthGenericRepository<T>:IGenericRepository<T> where T : BaseEntity
    {
        private readonly ApplicationIdentityDbContext _context;
        public AuthGenericRepository(ApplicationIdentityDbContext context)
        {
            _context = context;
        }

        public async Task Add(T entity)
        {
            await _context.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Remove(entity);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {

            return await _context.Set<T>().AsNoTracking().ToListAsync();

        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(P => P.Id == id);
        }
        public async Task<T?> GetByIdWithTrackingAsync(int id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(P => P.Id == id);
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
            _context.Update(entity);
        }

        private IQueryable<T> ApplySpecifications(ISpecifications<T> specifications)
        {
            return SpecificationsEvaluator<T>.GetQuery(_context.Set<T>(), specifications);
        }
    }
}
