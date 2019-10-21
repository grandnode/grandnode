using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Country
{
    public partial class CountriesListModel : BaseGrandModel
    {
        public CountriesListModel() { }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        public string CountryName { get; set; }

    }
}
