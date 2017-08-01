using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class QueuedEmailListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchEndDate { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.FromEmail")]
        
        public string SearchFromEmail { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.ToEmail")]
        
        public string SearchToEmail { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.LoadNotSent")]
        public bool SearchLoadNotSent { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.MaxSentTries")]
        public int SearchMaxSentTries { get; set; }

        [GrandResourceDisplayName("Admin.System.QueuedEmails.List.GoDirectlyToNumber")]
        public string GoDirectlyToNumber { get; set; }
    }
}