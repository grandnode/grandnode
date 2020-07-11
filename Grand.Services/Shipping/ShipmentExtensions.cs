using Grand.Domain.Shipping;
using Grand.Core.Html;
using System;

namespace Grand.Services.Shipping
{
    public static class ShipmentExtensions
    {
        /// <summary>
        /// Formats the order note text
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <returns>Formatted text</returns>
        public static string FormatOrderNoteText(this ShipmentNote shipmentNote)
        {
            if (shipmentNote == null)
                throw new ArgumentNullException("orderNote");

            string text = shipmentNote.Note;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }

    }
}
