using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    public class TwoFactorAuthenticationModel : BaseGrandModel
    {
        
        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        public string StatusMessage { get; set; }

    }
}
