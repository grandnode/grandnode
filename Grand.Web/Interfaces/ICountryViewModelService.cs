namespace Grand.Web.Interfaces
{
    public partial interface ICountryViewModelService
    {
        dynamic PrepareModel(string countryId, bool addSelectStateItem);

    }
}