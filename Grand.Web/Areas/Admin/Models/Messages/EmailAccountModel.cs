﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class EmailAccountModel : BaseEntityModel
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

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.UseServerCertificateValidation")]
        public bool UseServerCertificateValidation { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.SecureSocketOptions")]
        public int SecureSocketOptionsId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.IsDefaultEmailAccount")]
        public bool IsDefaultEmailAccount { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.SendTestEmailTo")]
        public string SendTestEmailTo { get; set; }

    }
}