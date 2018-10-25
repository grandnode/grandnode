//Contribution: Orchard project (https://github.com/OrchardCMS/OrchardCore)
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;

namespace Grand.Framework.TagHelpers
{
    public class ResourceManager : IResourceManager
    {
        private List<IHtmlContent> _headScripts;
        private List<IHtmlContent> _footScripts;

        public IEnumerable<IHtmlContent> GetRegisteredHeadScripts()
        {
            return _headScripts == null ? Enumerable.Empty<IHtmlContent>() : _headScripts;
        }

        public IEnumerable<IHtmlContent> GetRegisteredFootScripts()
        {
            return _footScripts == null ? Enumerable.Empty<IHtmlContent>() : _footScripts;
        }


        public void RegisterHeadScript(IHtmlContent script)
        {
            if (_headScripts == null)
            {
                _headScripts = new List<IHtmlContent>();
            }

            _headScripts.Add(script);
        }

        public void RegisterFootScript(IHtmlContent script)
        {
            if (_footScripts == null)
            {
                _footScripts = new List<IHtmlContent>();
            }

            _footScripts.Add(script);
        }

        public void RenderFootScript(IHtmlContentBuilder builder)
        {
            var first = true;
            foreach (var context in GetRegisteredFootScripts())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }
        public void RenderHeadScript(IHtmlContentBuilder builder)
        {
            var first = true;
            foreach (var context in GetRegisteredHeadScripts())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

    }
}
