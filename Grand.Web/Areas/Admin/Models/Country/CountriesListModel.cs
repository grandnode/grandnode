using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Country
{
    public partial class CountriesListModel : BaseModel
    {
        public CountriesListModel() { }

        [GrandResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        public string CountryName { get; set; }

    }
}
