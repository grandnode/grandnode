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

        public string Code { get; set; }

        public string UserUniqueKey { get; set; }

        public string QrCodeSetupImageUrl { get; set; }

        public string ManualInputCode { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
