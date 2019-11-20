using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel : BaseGrandModel
    {
        public ManufacturerNavigationModel()
        {
            Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
    }
}