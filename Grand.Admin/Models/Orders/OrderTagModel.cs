using Grand.Framework.Localization;
using Grand.Core.Models;
using Grand.Core.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Localization;

namespace Grand.Admin.Models.Orders
{
    public class OrderTagModel : BaseEntityModel, ILocalizedModel<OrderTagLocalizedModel>
    {
        public OrderTagModel()
        {
            Locales = new List<OrderTagLocalizedModel>();
        }

        [GrandResourceDisplayName("Admin.Orders.OrderTags.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Orders.OrderTags.Fields.OrderCount")]
        public int OrderCount { get; set; }
        public IList<OrderTagLocalizedModel> Locales { get; set; }
    }

    public partial class OrderTagLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }
        [GrandResourceDisplayName("Admin.Orders.OrderTags.Fields.Name")]
        public string Name { get; set; }

    }
}
