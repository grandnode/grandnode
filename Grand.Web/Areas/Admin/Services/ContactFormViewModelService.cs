using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ContactFormViewModelService : IContactFormViewModelService
    {
        private readonly IContactUsService _contactUsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IEmailAccountService _emailAccountService;

        public ContactFormViewModelService(IContactUsService contactUsService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreService storeService,
            IEmailAccountService emailAccountService)
        {
            this._contactUsService = contactUsService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeService = storeService;
            this._emailAccountService = emailAccountService;
        }

        public virtual ContactFormListModel PrepareContactFormListModel()
        {
            var model = new ContactFormListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return model;
        }

        public virtual (IEnumerable<ContactFormModel> contactFormModel, int totalCount) PrepareContactFormListModel(ContactFormListModel model, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.SearchStartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.SearchEndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            string vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var contactform = _contactUsService.GetAllContactUs(
                fromUtc: startDateValue,
                toUtc: endDateValue,
                email: model.SearchEmail,
                storeId: model.StoreId,
                vendorId: vendorId,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);

            return (contactform.Select(x => {
                    var store = _storeService.GetStoreById(x.StoreId);
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Enquiry = "";
                    m.Email = m.FullName + " - " + m.Email;
                    m.Store = store != null ? store.Name : "-empty-";
                    return m;
                }),contactform.TotalCount);
        }
        public virtual ContactFormModel PrepareContactFormModel(ContactUs contactUs)
        {
            var model = contactUs.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(contactUs.CreatedOnUtc, DateTimeKind.Utc);
            var store = _storeService.GetStoreById(contactUs.StoreId);
            model.Store = store != null ? store.Name : "-empty-";
            var email = _emailAccountService.GetEmailAccountById(contactUs.EmailAccountId);
            model.EmailAccountName = email != null ? email.DisplayName : "-empty-";
            return model;
        }
    }
}
