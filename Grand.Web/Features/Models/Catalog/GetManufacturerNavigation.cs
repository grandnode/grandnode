using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetManufacturerNavigation : IRequest<ManufacturerNavigationModel>
    {
        public string CurrentManufacturerId { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
    }
}
