using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

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
