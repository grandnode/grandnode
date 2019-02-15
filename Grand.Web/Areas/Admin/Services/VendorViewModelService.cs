using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Vendors;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class VendorViewModelService : IVendorViewModelService
    {
        private readonly IDiscountService _discountService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IEventPublisher _eventPublisher;
        private readonly VendorSettings _vendorSettings;

        public VendorViewModelService(IDiscountService discountService, IVendorService vendorService, ICustomerService customerService, ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper, ICountryService countryService, IStateProvinceService stateProvinceService, IStoreService storeService, IUrlRecordService urlRecordService,
            IPictureService pictureService, IEventPublisher eventPublisher, VendorSettings vendorSettings)
        {
            _discountService = discountService;
            _vendorService = vendorService;
            _customerService = customerService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _eventPublisher = eventPublisher;
            _vendorSettings = vendorSettings;
        }

        public virtual void PrepareDiscountModel(VendorModel model, Vendor vendor, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && vendor != null)
            {
                model.SelectedDiscountIds = vendor.AppliedDiscounts.ToArray();
            }
        }

        public virtual void PrepareVendorReviewModel(VendorReviewModel model,
            VendorReview vendorReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");
            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            var customer = _customerService.GetCustomerById(vendorReview.CustomerId);

            model.Id = vendorReview.Id;
            model.VendorId = vendorReview.VendorId;
            model.VendorName = vendor.Name;
            model.CustomerId = vendorReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = vendorReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorReview.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.Title = vendorReview.Title;
                if (formatReviewText)
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(vendorReview.ReviewText, false, true, false, false, false, false);
                else
                    model.ReviewText = vendorReview.ReviewText;
                model.IsApproved = vendorReview.IsApproved;
            }
        }

        public virtual void PrepareVendorAddressModel(VendorModel model, Vendor vendor)
        {

            if (model.Address == null)
                model.Address = new Models.Common.AddressModel();

            model.Address.FirstNameEnabled = false;
            model.Address.FirstNameRequired = false;
            model.Address.LastNameEnabled = false;
            model.Address.LastNameRequired = false;
            model.Address.EmailEnabled = false;
            model.Address.EmailRequired = false;
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
            foreach (var c in _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (vendor != null && c.Id == vendor.Address.CountryId) });

            var states = !String.IsNullOrEmpty(model.Address.CountryId) ? _stateProvinceService.GetStateProvincesByCountryId(model.Address.CountryId, showHidden: true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (vendor != null && s.Id == vendor.Address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
        }

        public virtual void PrepareStore(VendorModel model)
        {
            model.AvailableStores.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });

            foreach (var s in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString()
                });
            }
        }
        public virtual VendorModel PrepareVendorModel()
        {
            var model = new VendorModel();
            //discounts
            PrepareDiscountModel(model, null, true);
            //default values
            model.PageSize = 6;
            model.Active = true;
            model.AllowCustomersToSelectPageSize = true;
            model.PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions;

            //default value
            model.Active = true;

            //stores
            PrepareStore(model);

            //prepare address model
            PrepareVendorAddressModel(model, null);
            return model;
        }
        public virtual IList<VendorModel.AssociatedCustomerInfo> AssociatedCustomers(string vendorId)
        {
            return _customerService
                .GetAllCustomers(vendorId: vendorId)
                .Select(c => new VendorModel.AssociatedCustomerInfo()
                {
                    Id = c.Id,
                    Email = c.Email
                })
                .ToList();
        }
        public Vendor InsertVendorModel(VendorModel model)
        {
            var vendor = model.ToEntity();
            vendor.Address = model.Address.ToEntity();
            vendor.Address.CreatedOnUtc = DateTime.UtcNow;

            _vendorService.InsertVendor(vendor);

            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    vendor.AppliedDiscounts.Add(discount.Id);
            }

            //search engine name
            model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);
            vendor.Locales = model.Locales.ToLocalizedProperty(vendor, x => x.Name, _urlRecordService);
            vendor.SeName = model.SeName;
            _vendorService.UpdateVendor(vendor);

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(vendor.PictureId, vendor.Name);
            _urlRecordService.SaveSlug(vendor, model.SeName, "");

            return vendor;
        }
        public Vendor UpdateVendorModel(Vendor vendor, VendorModel model)
        {
            string prevPictureId = vendor.PictureId;
            vendor = model.ToEntity(vendor);
            vendor.Locales = model.Locales.ToLocalizedProperty(vendor, x => x.Name, _urlRecordService);
            model.SeName = vendor.ValidateSeName(model.SeName, vendor.Name, true);
            vendor.Address = model.Address.ToEntity(vendor.Address);

            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (vendor.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                        vendor.AppliedDiscounts.Add(discount.Id);
                }
                else
                {
                    //remove discount
                    if (vendor.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                        vendor.AppliedDiscounts.Remove(discount.Id);
                }
            }

            vendor.SeName = model.SeName;

            _vendorService.UpdateVendor(vendor);
            //search engine name                
            _urlRecordService.SaveSlug(vendor, model.SeName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != vendor.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(vendor.PictureId, vendor.Name);
            return vendor;
        }
        public void DeleteVendor(Vendor vendor)
        {
            //clear associated customer references
            var associatedCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                customer.VendorId = "";
                _customerService.UpdateCustomer(customer);
            }
            _vendorService.DeleteVendor(vendor);
        }
        public IList<VendorModel.VendorNote> PrepareVendorNote(Vendor vendor)
        {
            var vendorNoteModels = new List<VendorModel.VendorNote>();
            foreach (var vendorNote in vendor.VendorNotes
                .OrderByDescending(vn => vn.CreatedOnUtc))
            {
                vendorNoteModels.Add(new VendorModel.VendorNote
                {
                    Id = vendorNote.Id,
                    VendorId = vendor.Id,
                    Note = vendorNote.FormatVendorNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return vendorNoteModels;
        }
        public virtual bool InsertVendorNote(string vendorId, string message)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return false;

            var vendorNote = new VendorNote
            {
                Note = message,
                VendorId = vendorId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            vendor.VendorNotes.Add(vendorNote);
            _vendorService.UpdateVendor(vendor);

            return true;
        }
        public virtual void DeleteVendorNote(string id, string vendorId)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNote = vendor.VendorNotes.FirstOrDefault(vn => vn.Id == id);
            if (vendorNote == null)
                throw new ArgumentException("No vendor note found with the specified id");
            vendorNote.VendorId = vendor.Id;

            _vendorService.DeleteVendorNote(vendorNote);
        }

        public virtual (IEnumerable<VendorReviewModel> vendorReviewModels, int totalCount) PrepareVendorReviewModel(VendorReviewListModel model, int pageIndex, int pageSize)
        {
            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            IPagedList<VendorReview> vendorReviews = _vendorService.GetAllVendorReviews("", null,
                     createdOnFromValue, createdToFromValue, model.SearchText, model.SearchVendorId, pageIndex - 1, pageSize);

            return (vendorReviews.Select(x =>
                {
                    var m = new VendorReviewModel();
                    PrepareVendorReviewModel(m, x, false, true);
                    return m;
                }).ToList(), vendorReviews.TotalCount);
        }
        public virtual VendorReview UpdateVendorReviewModel(VendorReview vendorReview, VendorReviewModel model)
        {
            vendorReview.Title = model.Title;
            vendorReview.ReviewText = model.ReviewText;
            vendorReview.IsApproved = model.IsApproved;

            _vendorService.UpdateVendorReview(vendorReview);

            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            //update vendor totals
            _vendorService.UpdateVendorReviewTotals(vendor);
            return vendorReview;
        }
        public virtual void DeleteVendorReview(VendorReview vendorReview)
        {
            _vendorService.DeleteVendorReview(vendorReview);

            var vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            //update vendor totals
            _vendorService.UpdateVendorReviewTotals(vendor);
        }
        public virtual void ApproveVendorReviews(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idVendor = id.Split(':').Last().ToString();
                var vendor = _vendorService.GetVendorById(idVendor);
                var vendorReview = _vendorService.GetVendorReviewById(idReview);
                if (vendorReview != null)
                {
                    var previousIsApproved = vendorReview.IsApproved;
                    vendorReview.IsApproved = true;
                    _vendorService.UpdateVendorReview(vendorReview);
                    _vendorService.UpdateVendorReviewTotals(vendor);

                    //raise event (only if it wasn't approved before)
                    if (!previousIsApproved)
                        _eventPublisher.Publish(new VendorReviewApprovedEvent(vendorReview));
                }
            }
        }
        public virtual void DisapproveVendorReviews(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idVendor = id.Split(':').Last().ToString();

                var vendor = _vendorService.GetVendorById(idVendor);
                var vendorReview = _vendorService.GetVendorReviewById(idReview);
                if (vendorReview != null)
                {
                    vendorReview.IsApproved = false;
                    _vendorService.UpdateVendorReview(vendorReview);
                    _vendorService.UpdateVendorReviewTotals(vendor);
                }
            }
        }


    }
}
