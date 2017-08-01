
using Grand.Services.Payments;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Framework.Controllers
{
    /// <summary>
    /// Base controller for payment plugins
    /// </summary>
    public abstract class BasePaymentController : BasePluginController
    {
        //public abstract IList<string> ValidatePaymentForm(IFormCollection form);
        //public abstract ProcessPaymentRequest GetPaymentInfo(IFormCollection form);
    }
}
