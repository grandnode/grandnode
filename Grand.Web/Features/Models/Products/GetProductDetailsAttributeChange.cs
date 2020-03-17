using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductDetailsAttributeChange : IRequest<ProductDetailsAttributeChangeModel>
    {
        public Customer Customer { get; set; }
        public Currency Currency { get; set; }
        public Store Store { get; set; }
        public Product Product { get; set; }
        public bool ValidateAttributeConditions { get; set; }
        public bool LoadPicture { get; set; }
        public IFormCollection Form { get; set; }
    }
}
