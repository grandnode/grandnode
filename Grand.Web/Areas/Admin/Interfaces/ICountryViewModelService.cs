using Grand.Core.Domain.Directory;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICountryViewModelService
    {
        CountryModel PrepareCountryModel();
        Country InsertCountryModel(CountryModel model);
        Country UpdateCountryModel(Country country, CountryModel model);
        StateProvinceModel PrepareStateProvinceModel(string countryId);
        StateProvince InsertStateProvinceModel(StateProvinceModel model);
        StateProvince UpdateStateProvinceModel(StateProvince sp, StateProvinceModel model);
    }
}
