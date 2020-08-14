using Grand.Domain.Configuration;

namespace Grand.Domain.Common
{
    public class PdfSettings : ISettings
    {
        /// <summary>
        /// PDF logo picture identifier
        /// </summary>
        public string LogoPictureId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disallow customers to print PDF invoices for pedning orders
        /// </summary>
        public bool DisablePdfInvoicesForPendingOrders { get; set; }

        /// <summary>
        /// Gets or sets the text that will appear at the top of invoices 
        /// </summary>
        public string InvoiceHeaderText { get; set; }

        /// <summary>
        /// Gets or sets the text that will appear at the bottom of invoices
        /// </summary>
        public string InvoiceFooterText { get; set; }
    }
}