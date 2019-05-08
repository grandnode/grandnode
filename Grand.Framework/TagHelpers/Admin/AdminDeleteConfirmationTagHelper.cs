using Grand.Framework.Mvc.Models;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("admin-delete-confirmation")]
    public partial class AdminDeleteConfirmationTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("action-name")]
        public string Action { get; set; }

        [HtmlAttributeName("button-id")]
        public string ButtonId { get; set; }

        [HtmlAttributeName("id")]
        public string ModelId { get; set; }

        private readonly IHtmlHelper _htmlHelper;
        private readonly ILocalizationService _localizationService;

        public AdminDeleteConfirmationTagHelper(IHtmlHelper htmlHelper, ILocalizationService localizationService)
        {
            _htmlHelper = htmlHelper;
            _localizationService = localizationService;
        }

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Action))
                Action = "Delete";

            var windowId = new HtmlString(ViewContext.ViewData.ModelMetadata.ModelType.Name.ToLower() + "-delete-confirmation").ToHtmlString();

            var modelId = string.IsNullOrEmpty(ModelId) ? ViewContext.RouteData.Values["Id"].ToString() : ModelId;

            var deleteConfirmationModel = new DeleteConfirmationModel {
                Id = modelId,
                ControllerName = ViewContext.RouteData.Values["controller"].ToString(),
                ActionName = Action,
                WindowId = windowId
            };

            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("id", windowId);
            output.Attributes.SetAttribute("style", "display:none");

            output.Content.SetHtmlContent((await _htmlHelper.PartialAsync("Delete", deleteConfirmationModel)).ToHtmlString());

            var window = new StringBuilder();
            window.AppendLine("<script>");
            window.AppendLine("$(document).ready(function() {");
            window.AppendLine(string.Format("$('#{0}').click(function (e) ", ButtonId));
            window.AppendLine("{");
            window.AppendLine("e.preventDefault();");
            window.AppendLine(string.Format("var window = $('#{0}');", windowId));
            window.AppendLine("if (!window.data('kendoWindow')) {");
            window.AppendLine("window.kendoWindow({");
            window.AppendLine("modal: true,");
            window.AppendLine(string.Format("title: '{0}',", _localizationService.GetResource("Admin.Common.AreYouSure")));
            window.AppendLine("actions: ['Close']");
            window.AppendLine("});");
            window.AppendLine("}");
            window.AppendLine("window.data('kendoWindow').center().open();");
            window.AppendLine("});");
            window.AppendLine("});");
            window.AppendLine("</script>");
            output.PostContent.SetHtmlContent(window.ToString());

        }
    }
}
