using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Infrastructure._Identinty;
using Talabat.Infrastructure.Data;
using Talabat.Infrastructure.Generic_Repository;

namespace Talabat.Infrastructure
{
    public class AuthUnitOfWork : IAuthUnitOfWork
    {
        private readonly ApplicationIdentityDbContext _context;
        private Hashtable _repositories;
        public AuthUnitOfWork(ApplicationIdentityDbContext context)
        {
            _repositories = new Hashtable();
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var key = typeof(TEntity).Name;
            if (!_repositories.ContainsKey(key))
            {
                var repo = new AuthGenericRepository<TEntity>(_context);
                _repositories.Add(key, repo);
            }
            return _repositories[key] as IGenericRepository<TEntity>;
        }
    }
}
