using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Models.Customer
{
    public class QrCodeSetupModel
    {
        public string QrCodeImageUrl { get; set; }
        public string ManualEntryQrCode { get; set; }
    }
}
