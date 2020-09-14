﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorListModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Vendors.List.SearchName")]
        
        public string SearchName { get; set; }
    }
}