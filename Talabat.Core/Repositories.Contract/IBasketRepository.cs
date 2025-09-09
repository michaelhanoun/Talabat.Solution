using Talabat.Core.Entities.Basket;

namespace Talabat.Core.Repositories.Contract
{
    public interface IBasketRepository
    {
        Task<CustomerBasket?> GetBasketAsync(string basketId);

        Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket customerBasket);

        Task<bool> DeleteBasketAsync(string basketId);
    }
}
