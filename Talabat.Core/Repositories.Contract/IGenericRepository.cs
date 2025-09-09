using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.Repositories.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdWithTrackingAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetWithSpecAsync(ISpecifications<T> spec);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecifications<T> spec);
        Task<int> GetCountAsync(ISpecifications<T> spec);
        Task Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
