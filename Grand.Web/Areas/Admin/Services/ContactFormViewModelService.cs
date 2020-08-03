using Grand.Core;
using Grand.Domain.Messages;
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
using System.Threading.Tasks;

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
            _contactUsService = contactUsService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeService = storeService;
            _emailAccountService = emailAccountService;
        }

        public virtual async Task<ContactFormListModel> PrepareContactFormListModel()
        {
            var model = new ContactFormListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            return model;
        }

        public virtual async Task<(IEnumerable<ContactFormModel> contactFormModel, int totalCount)> PrepareContactFormListModel(ContactFormListModel model, int pageIndex, int pageSize)
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

            var contactform = await _contactUsService.GetAllContactUs(
                fromUtc: startDateValue,
                toUtc: endDateValue,
                email: model.SearchEmail,
                storeId: model.StoreId,
                vendorId: vendorId,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);
            var contactformmodelList = new List<ContactFormModel>();
            foreach (var item in contactform)
            {
                var store = await _storeService.GetStoreById(item.StoreId);
                var m = item.ToModel();
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc);
                m.Enquiry = "";
                m.Email = m.FullName + " - " + m.Email;
                m.Store = store != null ? store.Shortcut : "-empty-";
                contactformmodelList.Add(m);
            }

            return (contactformmodelList, contactform.TotalCount);
        }
        public virtual async Task<ContactFormModel> PrepareContactFormModel(ContactUs contactUs)
        {
            var model = contactUs.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(contactUs.CreatedOnUtc, DateTimeKind.Utc);
            var store = await _storeService.GetStoreById(contactUs.StoreId);
            model.Store = store != null ? store.Shortcut : "-empty-";
            var email = await _emailAccountService.GetEmailAccountById(contactUs.EmailAccountId);
            model.EmailAccountName = email != null ? email.DisplayName : "-empty-";
            return model;
        }
    }
}
