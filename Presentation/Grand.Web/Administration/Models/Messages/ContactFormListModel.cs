﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Admin.Models.Messages
{
    public partial class ContactFormListModel : BaseNopModel
    {
        public ContactFormListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.System.ContactForm.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchEndDate { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.List.Email")]
        [AllowHtml]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.System.ContactForm.List.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

    }
}