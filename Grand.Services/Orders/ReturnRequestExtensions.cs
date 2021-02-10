using Grand.Domain.Orders;
using Grand.Services.Common;
using System;

namespace Grand.Services.Orders
{
    public static class ReturnRequestExtensions
    {
        public static string FormatReturnRequestNoteText(this ReturnRequestNote returnRequestNote)
        {
            if (returnRequestNote == null)
                throw new ArgumentNullException("returnRequestNote");

            return FormatText.ConvertText(returnRequestNote.Note);
        }
    }
}
