namespace Grand.Web.Services
{
    public partial interface ICountryViewModelService
    {
        dynamic PrepareModel(string countryId, bool addSelectStateItem);

    }
}