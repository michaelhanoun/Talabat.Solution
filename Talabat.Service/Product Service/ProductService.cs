using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities.Product;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications.Product_Specs;

namespace Talabat.Application.Product_Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Product?> GetProductAsync(int productId)
        {
            var spec = new ProductWithBrandAndCategorySpecification(productId);
            var product = await _unitOfWork.Repository<Product>().GetWithSpecAsync(spec);
            return product;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(ProductSpecParams specParams) => await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(new ProductWithBrandAndCategorySpecification(specParams));

        
        public Task<IReadOnlyList<ProductBrand>> GetBrandsAsync() => _unitOfWork.Repository<ProductBrand>().GetAllAsync();
      

        public Task<IReadOnlyList<ProductCategory>> GetCategoriesAsync() => _unitOfWork.Repository<ProductCategory>().GetAllAsync();


        public async Task<int> GetCountAsync(ProductSpecParams specParams) => await _unitOfWork.Repository<Product>().GetCountAsync(new ProductsWithFilterationForCountSpecification( specParams));
    
    }
}
