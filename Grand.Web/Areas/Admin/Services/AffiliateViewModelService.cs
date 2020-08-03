using Grand.Core;
using Grand.Domain.Affiliates;
using Grand.Domain.Directory;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Seo;
using Grand.Domain.Shipping;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Affiliates;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class AffiliateViewModelService : IAffiliateViewModelService
    {

        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly SeoSettings _seoSettings;

        public AffiliateViewModelService(IWebHelper webHelper, IWorkContext workContext, ICountryService countryService, IStateProvinceService stateProvinceService,
            IPriceFormatter priceFormatter, IAffiliateService affiliateService,
            ICustomerService customerService, IOrderService orderService, ILocalizationService localizationService, IDateTimeHelper dateTimeHelper, SeoSettings seoSettings)
        {
            _webHelper = webHelper;
            _workContext = workContext;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _priceFormatter = priceFormatter;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _orderService = orderService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _seoSettings = seoSettings;
        }

        public virtual async Task PrepareAffiliateModel(AffiliateModel model, Affiliate affiliate, bool excludeProperties,
            bool prepareEntireAddressModel = true)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (affiliate != null)
            {
                model.Id = affiliate.Id;
                model.Url = affiliate.GenerateUrl(_webHelper);
                if (!excludeProperties)
                {
                    model.AdminComment = affiliate.AdminComment;
                    model.FriendlyUrlName = affiliate.FriendlyUrlName;
                    model.Active = affiliate.Active;
                    model.Address = await affiliate.Address.ToModel(_countryService, _stateProvinceService);
                }
            }

            if (prepareEntireAddressModel)
            {
                model.Address.FirstNameEnabled = true;
                model.Address.FirstNameRequired = true;
                model.Address.LastNameEnabled = true;
                model.Address.LastNameRequired = true;
                model.Address.EmailEnabled = true;
                model.Address.EmailRequired = true;
                model.Address.CompanyEnabled = true;
                model.Address.CountryEnabled = true;
                model.Address.StateProvinceEnabled = true;
                model.Address.CityEnabled = true;
                model.Address.CityRequired = true;
                model.Address.StreetAddressEnabled = true;
                model.Address.StreetAddressRequired = true;
                model.Address.StreetAddress2Enabled = true;
                model.Address.ZipPostalCodeEnabled = true;
                model.Address.ZipPostalCodeRequired = true;
                model.Address.PhoneEnabled = true;
                model.Address.PhoneRequired = true;
                model.Address.FaxEnabled = true;

                //address
                model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
                foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                    model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (affiliate != null && c.Id == affiliate.Address.CountryId) });

                var states = !String.IsNullOrEmpty(model.Address.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true) : new List<StateProvince>();
                if (states.Count > 0)
                {
                    foreach (var s in states)
                        model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (affiliate != null && s.Id == affiliate.Address.StateProvinceId) });
                }
                else
                    model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            }
        }

        public virtual async Task<(IEnumerable<AffiliateModel> affiliateModels, int totalCount)> PrepareAffiliateModelList(AffiliateListModel model, int pageIndex, int pageSize)
        {
            var affiliates = await _affiliateService.GetAllAffiliates(model.SearchFriendlyUrlName,
               model.SearchFirstName, model.SearchLastName,
               model.LoadOnlyWithOrders, model.OrdersCreatedFromUtc, model.OrdersCreatedToUtc,
               pageIndex - 1, pageSize, true);

            var affiliateModels = new List<AffiliateModel>();
            foreach (var x in affiliates)
            {
                var m = new AffiliateModel();
                await PrepareAffiliateModel(m, x, false, false);
                affiliateModels.Add(m);
            }
            return (affiliateModels, affiliates.TotalCount);
        }
        public virtual async Task<Affiliate> InsertAffiliateModel(AffiliateModel model)
        {
            var affiliate = new Affiliate();
            affiliate.Active = model.Active;
            affiliate.AdminComment = model.AdminComment;
            //validate friendly URL name
            var friendlyUrlName = await affiliate.ValidateFriendlyUrlName(_affiliateService, _seoSettings, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.Address = model.Address.ToEntity();
            affiliate.Address.CreatedOnUtc = DateTime.UtcNow;
            //some validation
            await _affiliateService.InsertAffiliate(affiliate);
            return affiliate;
        }
        public virtual async Task<Affiliate> UpdateAffiliateModel(AffiliateModel model, Affiliate affiliate)
        {
            affiliate.Active = model.Active;
            affiliate.AdminComment = model.AdminComment;
            //validate friendly URL name
            var friendlyUrlName = await affiliate.ValidateFriendlyUrlName(_affiliateService, _seoSettings, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.Address = model.Address.ToEntity(affiliate.Address);
            await _affiliateService.UpdateAffiliate(affiliate);
            return affiliate;
        }
        public virtual async Task<(IEnumerable<AffiliateModel.AffiliatedOrderModel> affiliateOrderModels, int totalCount)> PrepareAffiliatedOrderList(Affiliate affiliate, AffiliatedOrderListModel model, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var orders = await _orderService.SearchOrders(
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                affiliateId: affiliate.Id,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);

            return (orders.Select(order =>
                {
                    var orderModel = new AffiliateModel.AffiliatedOrderModel();
                    orderModel.Id = order.Id;
                    orderModel.OrderNumber = order.OrderNumber;
                    orderModel.OrderCode = order.Code;
                    orderModel.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
                    orderModel.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
                    orderModel.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
                    orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
                    orderModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
                    return orderModel;
                }), orders.TotalCount);
        }

        public virtual async Task<(IEnumerable<AffiliateModel.AffiliatedCustomerModel> affiliateCustomerModels, int totalCount)> PrepareAffiliatedCustomerList(Affiliate affiliate, int pageIndex, int pageSize)
        {
            var customers = await _customerService.GetAllCustomers(
                affiliateId: affiliate.Id,
                pageIndex: pageIndex - 1,
                pageSize: pageSize);

            return (customers.Select(customer =>
                {
                    var customerModel = new AffiliateModel.AffiliatedCustomerModel();
                    customerModel.Id = customer.Id;
                    customerModel.Name = customer.Email;
                    return customerModel;
                }), customers.TotalCount);
        }

    }
}
