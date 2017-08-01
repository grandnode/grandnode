using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class ContactFormListModel : BaseGrandModel
    {
        public ContactFormListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
        [GrandResourceDisplayName("Admin.System.ContactForm.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchEndDate { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.List.Email")]
        
        public string SearchEmail { get; set; }

        [GrandResourceDisplayName("Admin.System.ContactForm.List.Store")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

    }
}