using Grand.Domain.Orders;
using Grand.Core.Html;
using System;

namespace Grand.Services.Orders
{
    public static class ReturnRequestExtensions
    {
        public static string FormatReturnRequestNoteText(this ReturnRequestNote returnRequestNote)
        {
            if (returnRequestNote == null)
                throw new ArgumentNullException("returnRequestNote");

            string text = returnRequestNote.Note;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }
    }
}
