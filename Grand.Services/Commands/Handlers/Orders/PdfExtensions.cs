using Grand.Core;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Grand.Services.Commands.Handlers.Orders
{
    public static class PdfExtensions
    {
        public static Font GetFont(string fontFileName)
        {
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            string fontPath = Path.Combine(CommonHelper.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);

            return font;
        }

        public static Font PrepareTitleFont(string fontFileName)
        {
            //fonts
            var titleFont = GetFont(fontFileName);
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            return titleFont;
        }
    }
}
