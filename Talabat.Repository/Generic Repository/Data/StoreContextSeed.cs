using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Infrastructure.Generic_Repository.Data
{
    public class StoreContextSeed
    {
        public async static Task SeedAsync(StoreContext _storeContext)
        {
            if (_storeContext.ProductBrands.Count() == 0)
            {
            var brandsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/brands.json");
            var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
            if(brands?.Count()>0)
            {
                    _storeContext.Set<ProductBrand>().AddRange(brands);
                     await _storeContext.SaveChangesAsync(); 
            }
            }

            if (_storeContext.ProductCategories.Count() == 0)
            {
                var categoriesData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/categories.json");
                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);
                if (categories?.Count() > 0)
                {
                    _storeContext.Set<ProductCategory>().AddRange(categories);
                    await _storeContext.SaveChangesAsync();
                }
            }

            if (_storeContext.Products.Count() == 0)
            {
                var productsData = File.ReadAllText("../Talabat.Repository/Data/DataSeed/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                if (products?.Count() > 0)
                {
                    _storeContext.Set<Product>().AddRange(products);
                    await _storeContext.SaveChangesAsync();
                }
            }
        }
    }
}
