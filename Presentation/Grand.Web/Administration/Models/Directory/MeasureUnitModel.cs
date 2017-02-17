using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Directory;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Directory
{
    [Validator(typeof(MeasureUnitValidator))]
    public partial class MeasureUnitModel : BaseNopEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Measures.Units.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Units.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}