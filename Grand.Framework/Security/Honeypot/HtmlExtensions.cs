using Grand.Core.Domain.Security;
using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;

namespace Grand.Framework.Security.Honeypot
{
    public static class HtmlExtensions
    {
        public static IHtmlContent GenerateHoneypotInput(this IHtmlHelper helper)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("<div style=\"display:none;\">");
            sb.Append(Environment.NewLine);

            var securitySettings = EngineContext.Current.Resolve<SecuritySettings>();
            sb.AppendFormat("<input id=\"{0}\" name=\"{0}\" type=\"text\">", securitySettings.HoneypotInputName);

            sb.Append(Environment.NewLine);
            sb.Append("</div>");

            return new HtmlString(sb.ToString());
        }
    }
}