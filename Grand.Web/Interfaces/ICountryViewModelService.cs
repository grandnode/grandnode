using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICountryViewModelService
    {
        Task<dynamic> PrepareModel(string countryId, bool addSelectStateItem);

    }
}