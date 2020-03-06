using Grand.Core.Domain.Customers;
using Grand.Web.Models.Common;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICommonViewModelService
    {
        Task<LogoModel> PrepareLogo();
        Task<LanguageSelectorModel> PrepareLanguageSelector();
        Task<CurrencySelectorModel> PrepareCurrencySelector();
        Task SetCurrency(string customerCurrency);
        TaxTypeSelectorModel PrepareTaxTypeSelector();
        Task SetTaxType(int customerTaxType);
        Task<StoreSelectorModel> PrepareStoreSelector();
        Task SetStore(string storeid);
        Task<int> GetUnreadPrivateMessages();
        Task<HeaderLinksModel> PrepareHeaderLinks(Customer customer);
        Task<ShoppingCartLinksModel> PrepareShoppingCartLinks(Customer customer);
        Task<AdminHeaderLinksModel> PrepareAdminHeaderLinks(Customer customer);
        Task<FooterModel> PrepareFooter();
        StoreThemeSelectorModel PrepareStoreThemeSelector();
        FaviconModel PrepareFavicon();
        Task<string> PrepareRobotsTextFile();
    }
}