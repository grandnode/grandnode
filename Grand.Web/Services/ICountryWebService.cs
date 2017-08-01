namespace Grand.Web.Services
{
    public partial interface ICountryWebService
    {
        dynamic PrepareModel(string countryId, bool addSelectStateItem);

    }
}