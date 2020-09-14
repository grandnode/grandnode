using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel : BaseModel
    {
        public ManufacturerNavigationModel()
        {
            Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : BaseEntityModel
    {
        public string Name { get; set; }
        public string SeName { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
    }
}