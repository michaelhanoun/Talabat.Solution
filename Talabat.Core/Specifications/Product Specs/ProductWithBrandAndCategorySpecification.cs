using Talabat.Core.Entities.Product;
namespace Talabat.Core.Specifications.Product_Specs
{
    public class ProductWithBrandAndCategorySpecification : BaseSpecifications<Product>
    {
        public ProductWithBrandAndCategorySpecification(ProductSpecParams specParams):base(P=> (!specParams.BrandId.HasValue||P.BrandId == specParams.BrandId.Value)
        &&(!specParams.CategoryId.HasValue || P.CategoryId == specParams.CategoryId.Value)&&(string.IsNullOrEmpty(specParams.Search)||P.Name.Contains( specParams.Search))
        )
        {
            Add(specParams);
          
        }


        public ProductWithBrandAndCategorySpecification(int id) : base(P=>P.Id == id)
        {
            AddIncludes();
        }
        private void AddIncludes()
        {
            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);
        }
        private void Add(ProductSpecParams specParams)
        {
            AddIncludes();
            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "priceAsc":
                        AddOrderBy(P => P.Price);
                        break;
                    case "priceDesc":
                        AddOrderByDesc(P => P.Price);
                        break;
                    default:
                        AddOrderBy(P => P.Name);
                        break;
                }
            }
            else
                AddOrderBy(P => P.Name);
            ApplyPagination((specParams.PageIndex-1)*specParams.PageSize,specParams.PageSize);
        }
    }
}
