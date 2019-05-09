using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        Task<ContactUsModel> PrepareContactUs();
        Task<ContactUsModel> SendContactUs(ContactUsModel model);
        Task<ContactVendorModel> PrepareContactVendor(Vendor vendor);
        Task<ContactVendorModel> SendContactVendor(ContactVendorModel model, Vendor vendor);
        Task<SitemapModel> PrepareSitemap();
        Task<string> SitemapXml(int? id, IUrlHelper url);
        StoreThemeSelectorModel PrepareStoreThemeSelector();
        FaviconModel PrepareFavicon();
        Task<string> PrepareRobotsTextFile();
        Task<string> ParseContactAttributes(IFormCollection form);
        Task<IList<string>> GetContactAttributesWarnings(string contactAttributesXml);
        Task<IList<ContactUsModel.ContactAttributeModel>> PrepareContactAttributeModel(string selectedContactAttributes);
    }
}