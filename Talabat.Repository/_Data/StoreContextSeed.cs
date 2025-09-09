using System.Data;
using System.Text.Json;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Entities.Product;

namespace Talabat.Infrastructure.Data
{
    public class StoreContextSeed
    {
        public async static Task SeedAsync(StoreContext _storeContext)
        {
            if (_storeContext.ProductBrands.Count() == 0)
            {
            var brandsData = File.ReadAllText("../Talabat.Repository/_Data/DataSeed/brands.json");
            var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
            if(brands?.Count()>0)
            {
                    _storeContext.Set<ProductBrand>().AddRange(brands);
                     await _storeContext.SaveChangesAsync(); 
            }
            }

            if (_storeContext.ProductCategories.Count() == 0)
            {
                var categoriesData = File.ReadAllText("../Talabat.Repository/_Data/DataSeed/categories.json");
                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);
                if (categories?.Count() > 0)
                {
                    _storeContext.Set<ProductCategory>().AddRange(categories);
                    await _storeContext.SaveChangesAsync();
                }
            }

            if (_storeContext.Products.Count() == 0)
            {
                var productsData = File.ReadAllText("../Talabat.Repository/_Data/DataSeed/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                if (products?.Count() > 0)
                {
                    _storeContext.Set<Product>().AddRange(products);
                    await _storeContext.SaveChangesAsync();
                }
            }
            if (_storeContext.DeliveryMethods.Count() == 0)
            {
            var deliveryData = File.ReadAllText("../Talabat.Repository/_Data/DataSeed/delivery.json");
                var delivery = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryData);
                if (delivery?.Count() > 0)
                {
                    _storeContext.Set<DeliveryMethod>().AddRange(delivery);
                    await _storeContext.SaveChangesAsync();
                }
            }
        }
    }
}
