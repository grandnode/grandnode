using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel : BaseGrandModel
    {
        public ManufacturerNavigationModel()
        {
            this.Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }
        
        public bool IsActive { get; set; }
    }
}