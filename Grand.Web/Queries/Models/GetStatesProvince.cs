using Grand.Web.Models.Common;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Queries.Models
{
    public class GetStatesProvince : IRequest<IList<StateProvinceModel>>
    {
        public string CountryId { get; set; }
        public bool AddSelectStateItem { get; set; }
    }
}
