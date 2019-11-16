using Grand.Web.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICountryViewModelService
    {
        Task<List<StateProvinceModel>> PrepareModel(string countryId, bool addSelectStateItem);
    }
}