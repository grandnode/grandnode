using MediatR;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace Grand.Framework.Events
{
    /// <summary>
    /// Admin tabstrip created event
    /// </summary>
    public class AdminTabStripCreated : INotification
    {
        public AdminTabStripCreated(string tabStripName)
        {
            this.TabStripName = tabStripName;
            this.BlocksToRender = new List<(string tabname, IHtmlContent content)>();
        }

        public string TabStripName { get; private set; }
        public IList<(string tabname, IHtmlContent content)> BlocksToRender { get; set; }
    }
}
