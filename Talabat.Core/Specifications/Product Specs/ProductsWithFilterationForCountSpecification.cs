using Talabat.Core.Entities.Product;

namespace Talabat.Core.Specifications.Product_Specs
{
    public class ProductsWithFilterationForCountSpecification : BaseSpecifications<Product>
    {
       
        public ProductsWithFilterationForCountSpecification(ProductSpecParams specParams):base(P=> (!specParams.BrandId.HasValue||P.BrandId == specParams.BrandId)
        &&(!specParams.CategoryId.HasValue||P.CategoryId == specParams.CategoryId)&&(string.IsNullOrEmpty(specParams.Search)||P.Name.Contains(specParams.Search)))
        {
        }

    }
}
