using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Common;

namespace Grand.Web.Areas.Admin.Models.Common
{
    [Validator(typeof(GenericAttributeValidator))]
    public partial class GenericAttributeModel
    {
        public string Id { get; set; }
        public string ObjectType { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string StoreId { get; set; }
    }
}