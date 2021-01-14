using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Shipping
{
    public partial class DeliveryDateModel : BaseEntityModel, ILocalizedModel<DeliveryDateLocalizedModel>
    {
        public DeliveryDateModel()
        {
            Locales = new List<DeliveryDateLocalizedModel>();
        }
        [GrandResourceDisplayName("Admin.Configuration.Shipping.DeliveryDates.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.DeliveryDates.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.DeliveryDates.Fields.ColorSquaresRgb")]

        public string ColorSquaresRgb { get; set; }

        public IList<DeliveryDateLocalizedModel> Locales { get; set; }
    }

    public partial class DeliveryDateLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.DeliveryDates.Fields.Name")]

        public string Name { get; set; }

    }
}