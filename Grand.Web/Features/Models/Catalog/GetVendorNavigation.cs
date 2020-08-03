using Grand.Domain.Localization;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetVendorNavigation : IRequest<VendorNavigationModel>
    {
        public Language Language { get; set; }
    }
}
