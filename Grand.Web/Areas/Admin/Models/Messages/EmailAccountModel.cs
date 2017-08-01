using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(EmailAccountValidator))]
    public partial class EmailAccountModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Email")]
        
        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.DisplayName")]
        
        public string DisplayName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Host")]
        
        public string Host { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Port")]
        public int Port { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Username")]
        
        public string Username { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Password")]
        
        public string Password { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.EnableSsl")]
        public bool EnableSsl { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.UseDefaultCredentials")]
        public bool UseDefaultCredentials { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.IsDefaultEmailAccount")]
        public bool IsDefaultEmailAccount { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.SendTestEmailTo")]
        
        public string SendTestEmailTo { get; set; }

    }
}