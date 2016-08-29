﻿using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Vendors;
using Grand.Web.Framework;
using Grand.Web.Framework.Localization;
using Grand.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;
using System;

namespace Grand.Admin.Models.Vendors
{
    [Validator(typeof(VendorValidator))]
    public partial class VendorModel : BaseNopEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        public VendorModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<AssociatedCustomerInfo>();
        }

        [NopResourceDisplayName("Admin.Vendors.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Vendors.Fields.Picture")]
        public string PictureId { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        public IList<VendorLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.AssociatedCustomerEmails")]
        public IList<AssociatedCustomerInfo> AssociatedCustomers { get; set; }



        //vendor notes
        [NopResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
        [AllowHtml]
        public string AddVendorNoteMessage { get; set; }




        #region Nested classes

        public class AssociatedCustomerInfo : BaseNopEntityModel
        {
            public string Email { get; set; }
        }


        public partial class VendorNote : BaseNopEntityModel
        {
            public string VendorId { get; set; }
            [NopResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
            public string Note { get; set; }
            [NopResourceDisplayName("Admin.Vendors.VendorNotes.Fields.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }
        #endregion

    }

    public partial class VendorLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Vendors.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}