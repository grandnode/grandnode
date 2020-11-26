using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public class LocationModel : BaseModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
