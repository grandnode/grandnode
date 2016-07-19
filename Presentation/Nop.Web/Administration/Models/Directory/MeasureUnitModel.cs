using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Directory;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Directory
{
    [Validator(typeof(MeasureUnitValidator))]
    public partial class MeasureUnitModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.Measures.Units.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Measures.Units.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}