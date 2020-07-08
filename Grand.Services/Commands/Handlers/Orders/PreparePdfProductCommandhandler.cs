using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Core.Html;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class PreparePdfProductCommandHandler : IRequestHandler<PreparePdfProductCommand, PdfPTable>
    {
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly IMeasureService _measureService;
        private readonly PdfSettings _pdfSettings;
        private readonly MeasureSettings _measureSettings;

        public PreparePdfProductCommandHandler(
            IProductService productService,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IStoreContext storeContext,
            IMeasureService measureService,
            PdfSettings pdfSettings,
            MeasureSettings measureSettings)
        {
            _productService = productService;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _measureService = measureService;
            _pdfSettings = pdfSettings;
            _measureSettings = measureSettings;
        }

        public async Task<PdfPTable> Handle(PreparePdfProductCommand request, CancellationToken cancellationToken)
        {
            return await PrepareProducts(request.Product, request.Language, request.ProductNumber);
        }

        protected virtual async Task<PdfPTable> PrepareProducts(Product product, Language language, int productNumber)
        {
            var productName = product.GetLocalized(x => x.Name, language.Id);
            var productDescription = product.GetLocalized(x => x.FullDescription, language.Id);

            var font = PdfExtensions.GetFont(_pdfSettings.FontFileName);
            var titleFont = PdfExtensions.PrepareTitleFont(_pdfSettings.FontFileName);

            var productTable = new PdfPTable(1);
            productTable.WidthPercentage = 100f;
            productTable.DefaultCell.Border = Rectangle.NO_BORDER;
            if (language.Rtl)
            {
                productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }

            productTable.AddCell(new Paragraph(String.Format("{0}. {1}", productNumber, productName), titleFont));
            productTable.AddCell(new Paragraph(" "));
            productTable.AddCell(new Paragraph(HtmlHelper.StripTags(HtmlHelper.ConvertHtmlToPlainText(productDescription, decode: true)), font));
            productTable.AddCell(new Paragraph(" "));

            if (product.ProductType != ProductType.GroupedProduct)
            {
                //simple product
                //render its properties such as price, weight, etc
                var priceStr = string.Format("{0} {1}", product.Price.ToString("0.00"), (await _currencyService.GetPrimaryStoreCurrency()).CurrencyCode);
                if (product.ProductType == ProductType.Reservation)
                    priceStr = _priceFormatter.FormatReservationProductPeriod(product, priceStr);
                productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.Price", language.Id), priceStr), font));
                productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.SKU", language.Id), product.Sku), font));

                if (product.IsShipEnabled && product.Weight > Decimal.Zero)
                    productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Weight", language.Id), product.Weight.ToString("0.00"), (await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)).Name), font));

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.StockQuantity", language.Id), product.GetTotalStockQuantity(warehouseId: _storeContext.CurrentStore.DefaultWarehouseId)), font));

                productTable.AddCell(new Paragraph(" "));
            }
            var pictures = product.ProductPictures;
            if (pictures.Count > 0)
            {
                var table = new PdfPTable(2);
                table.WidthPercentage = 100f;
                if (language.Rtl)
                {
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                foreach (var pic in pictures)
                {
                    var pp = await _pictureService.GetPictureById(pic.PictureId);
                    if (pp != null)
                    {
                        var picBinary = await _pictureService.LoadPictureBinary(pp);
                        if (picBinary != null && picBinary.Length > 0)
                        {
                            var pictureLocalPath = await _pictureService.GetThumbLocalPath(pp, 200, false);
                            var cell = new PdfPCell(Image.GetInstance(pictureLocalPath));
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.Border = Rectangle.NO_BORDER;
                            table.AddCell(cell);
                        }
                    }
                }

                if (pictures.Count % 2 > 0)
                {
                    var cell = new PdfPCell(new Phrase(" "));
                    cell.Border = Rectangle.NO_BORDER;
                    table.AddCell(cell);
                }

                productTable.AddCell(table);
                productTable.AddCell(new Paragraph(" "));
            }


            if (product.ProductType == ProductType.GroupedProduct)
            {
                //grouped product. render its associated products
                int pvNum = 1;
                var associatedProducts = await _productService.GetAssociatedProducts(product.Id, showHidden: true);
                foreach (var associatedProduct in associatedProducts)
                {
                    productTable.AddCell(new Paragraph(String.Format("{0}-{1}. {2}", productNumber, pvNum, associatedProduct.GetLocalized(x => x.Name, language.Id)), font));
                    productTable.AddCell(new Paragraph(" "));

                    productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Price", language.Id), associatedProduct.Price.ToString("0.00"), (await _currencyService.GetPrimaryStoreCurrency()).CurrencyCode), font));
                    productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.SKU", language.Id), associatedProduct.Sku), font));

                    if (associatedProduct.IsShipEnabled && associatedProduct.Weight > Decimal.Zero)
                        productTable.AddCell(new Paragraph(String.Format("{0}: {1} {2}", _localizationService.GetResource("PDFProductCatalog.Weight", language.Id), associatedProduct.Weight.ToString("0.00"), (await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)).Name), font));

                    if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph(String.Format("{0}: {1}", _localizationService.GetResource("PDFProductCatalog.StockQuantity", language.Id), associatedProduct.GetTotalStockQuantity()), font));

                    productTable.AddCell(new Paragraph(" "));

                    pvNum++;
                }
            }
            return productTable;
        }

    }
}
