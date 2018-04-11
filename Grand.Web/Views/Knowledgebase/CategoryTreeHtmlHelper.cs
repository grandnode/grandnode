using Grand.Core.Html;
using Grand.Web.Models.Knowledgebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Views.Knowledgebase
{
    public static class CategoryTreeHtmlHelper
    {
        public static string ShowSubItems(KnowledgebaseCategoryModel _object)
        {
            StringBuilder output = new StringBuilder();
            //if (_object.Children.Count > 0)
            //{
            output.Append("<ul>");
            output.Append("<li>");
            output.Append(_object.Name);
            output.Append("</li>");

            foreach (KnowledgebaseCategoryModel subItem in _object.Children)
            {
                output.Append(CategoryTreeHtmlHelper.ShowSubItems(subItem));
            }

            output.Append("</ul>");
            //}

            return output.ToString();
        }
    }
}
