//Contribution: Orchard project (https://github.com/OrchardCMS/OrchardCore)
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace Grand.Framework.TagHelpers
{
    public interface IResourceManager
    {
        /// <summary>
        /// Registers a custom script tag on at the head.
        /// </summary>
        void RegisterHeadScript(IHtmlContent script);

        /// <summary>
        /// Registers a custom script tag on at the foot.
        /// </summary>
        /// <param name="script"></param>
        void RegisterFootScript(IHtmlContent script);

        /// <summary>
        /// Returns the registered header script resources.
        /// </summary>
        IEnumerable<IHtmlContent> GetRegisteredHeadScripts();

        /// <summary>
        /// Returns the registered footer script resources.
        /// </summary>
        IEnumerable<IHtmlContent> GetRegisteredFootScripts();

        /// <summary>
        /// Renders the registered header script tags.
        /// </summary>
        void RenderHeadScript(IHtmlContentBuilder builder);

        /// <summary>
        /// Renders the registered footer script tags.
        /// </summary>
        void RenderFootScript(IHtmlContentBuilder builder);
    }
}
