using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Vendors;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Framework.Localization;
using System.ComponentModel.DataAnnotations;
using System;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Web.Areas.Admin.Models.Stores;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    [Validator(typeof(VendorValidator))]
    public partial class VendorModel : BaseGrandEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        public VendorModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<AssociatedCustomerInfo>();
            Address = new AddressModel();
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Email")]
        
        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Description")]
        
        public string Description { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.Vendors.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Store")]
        public string StoreId { get; set; }
        public List<SelectListItem> AvailableStores { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AdminComment")]
        
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Active")]
        public bool Active { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.SeName")]
        
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.PageSize")]
        public int PageSize { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        public IList<VendorLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AssociatedCustomerEmails")]
        public IList<AssociatedCustomerInfo> AssociatedCustomers { get; set; }

        public AddressModel Address { get; set; }

        //vendor notes
        [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
        
        public string AddVendorNoteMessage { get; set; }

        public List<DiscountModel> AvailableDiscounts { get; set; }
        public string[] SelectedDiscountIds { get; set; }


        #region Nested classes

        public class AssociatedCustomerInfo : BaseGrandEntityModel
        {
            public string Email { get; set; }
        }


        public partial class VendorNote : BaseGrandEntityModel
        {
            public string VendorId { get; set; }
            [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
            public string Note { get; set; }
            [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }
        #endregion

    }

    public partial class VendorLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Description")]
        
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        
        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        
        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        
        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.SeName")]
        
        public string SeName { get; set; }
    }
}