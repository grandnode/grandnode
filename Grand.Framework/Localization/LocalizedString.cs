using Microsoft.AspNetCore.Html;

namespace Grand.Framework.Localization
{
    public class LocalizedString : HtmlString
    {
        private readonly string _localized;

        public LocalizedString(string localized): base (localized)
        {
            _localized = localized;
        }
        
        public string Text
        {
            get { return _localized; }
        }
    }
}