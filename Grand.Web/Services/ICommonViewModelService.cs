using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Grand.Web.Services
{
    public partial interface ICommonViewModelService
    {
        LogoModel PrepareLogo();
        LanguageSelectorModel PrepareLanguageSelector();
        void SetLanguage(string langid);
        CurrencySelectorModel PrepareCurrencySelector();
        void SetCurrency(string customerCurrency);
        TaxTypeSelectorModel PrepareTaxTypeSelector();
        void SetTaxType(int customerTaxType);
        StoreSelectorModel PrepareStoreSelector();
        void SetStore(string storeid);
        int GetUnreadPrivateMessages();
        HeaderLinksModel PrepareHeaderLinks(Customer customer);
        AdminHeaderLinksModel PrepareAdminHeaderLinks(Customer customer);
        FooterModel PrepareFooter();
        ContactUsModel PrepareContactUs();
        ContactUsModel SendContactUs(ContactUsModel model);
        ContactVendorModel PrepareContactVendor(Vendor vendor);
        ContactVendorModel SendContactVendor(ContactVendorModel model, Vendor vendor);
        SitemapModel PrepareSitemap();
        string SitemapXml(int? id, IUrlHelper url);
        StoreThemeSelectorModel PrepareStoreThemeSelector();
        FaviconModel PrepareFavicon();
        string PrepareRobotsTextFile();
        string ParseContactAttributes(IFormCollection form);
        IList<string> GetContactAttributesWarnings(string contactAttributesXml);
        IList<ContactUsModel.ContactAttributeModel> PrepareContactAttributeModel(string selectedContactAttributes);
    }
}