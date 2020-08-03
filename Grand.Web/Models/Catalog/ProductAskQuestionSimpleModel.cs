using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductAskQuestionSimpleModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Products.AskQuestion.Email")]
        public string AskQuestionEmail { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.FullName")]
        public string AskQuestionFullName { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.Phone")]
        public string AskQuestionPhone { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.Message")]
        public string AskQuestionMessage { get; set; }

        public bool DisplayCaptcha { get; set; }

    }
}