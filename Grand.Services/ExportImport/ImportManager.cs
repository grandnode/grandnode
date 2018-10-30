using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.ExportImport.Help;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Microsoft.AspNetCore.StaticFiles;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Grand.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IDownloadService _downloadService;
        private readonly IShippingService _shippingService;
        private readonly ITaxCategoryService _taxService;
        private readonly IMeasureService _measureService;
        #endregion

        #region Ctor

        public ImportManager(IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IVendorService vendorService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductTemplateService productTemplateService,
            IDownloadService downloadService,
            IShippingService shippingService,
            ITaxCategoryService taxService,
            IMeasureService measureService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._urlRecordService = urlRecordService;
            this._storeContext = storeContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._vendorService = vendorService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._productTemplateService = productTemplateService;
            this._downloadService = downloadService;
            this._shippingService = shippingService;
            this._taxService = taxService;
            this._measureService = measureService;
        }

        #endregion

        #region Utilities

        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out string mimeType);
            //set to jpeg in case mime type cannot be found
            if (mimeType == null)
                mimeType = "image/jpeg";
            return mimeType;
        }

        /// <summary>
        /// Creates or loads the image
        /// </summary>
        /// <param name="picturePath">The path to the image file</param>
        /// <param name="name">The name of the object</param>
        /// <param name="picId">Image identifier, may be null</param>
        /// <returns>The image or null if the image has not changed</returns>
        protected virtual Picture LoadPicture(string picturePath, string name, string picId = "")
        {
            if (String.IsNullOrEmpty(picturePath) || !File.Exists(picturePath))
                return null;

            var mimeType = GetMimeTypeFromFilePath(picturePath);
            var newPictureBinary = File.ReadAllBytes(picturePath);
            var pictureAlreadyExists = false;
            if (!String.IsNullOrEmpty(picId))
            {
                //compare with existing product pictures
                var existingPicture = _pictureService.GetPictureById(picId);

                var existingBinary = _pictureService.LoadPictureBinary(existingPicture);
                //picture binary after validation (like in database)
                var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                if (existingBinary.SequenceEqual(validatedPictureBinary) ||
                    existingBinary.SequenceEqual(newPictureBinary))
                {
                    pictureAlreadyExists = true;
                }
            }

            if (pictureAlreadyExists) return null;

            var newPicture = _pictureService.InsertPicture(newPictureBinary, mimeType,
                _pictureService.GetPictureSeName(name));
            return newPicture;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual void ImportProductsFromXlsx(Stream stream)
        {

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new GrandException("No worksheet found");


                //the columns
                var properties = new List<PropertyByName<Product>>();
                var poz = 1;
                while (true)
                {
                    try
                    {
                        var cell = worksheet.Cells[1, poz];

                        if (cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()))
                            break;

                        poz += 1;
                        properties.Add(new PropertyByName<Product>(cell.Value.ToString().ToLower()));
                    }
                    catch
                    {
                        break;
                    }
                }

                var manager = new PropertyManager<Product>(properties.ToArray());

                var templates = _productTemplateService.GetAllProductTemplates();
                var deliveryDates = _shippingService.GetAllDeliveryDates();
                var taxes = _taxService.GetAllTaxCategories();
                var warehouses = _shippingService.GetAllWarehouses();
                var units = _measureService.GetAllMeasureUnits();

                int iRow = 2;
                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                                            .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                                            .All(cell => cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    manager.ReadFromXlsx(worksheet, iRow);
                    var sku = manager.GetProperty("sku") != null ? manager.GetProperty("sku").StringValue : string.Empty;
                    var productid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;

                    Product product = null;

                    if(!String.IsNullOrEmpty(sku))
                        product = _productService.GetProductBySku(sku);

                    if(!String.IsNullOrEmpty(productid))
                        product = _productService.GetProductById(productid);

                    var isNew = product == null;

                    product = product ?? new Product();
                    
                    if (isNew)
                    {
                        product.CreatedOnUtc = DateTime.UtcNow;
                        product.ProductTemplateId = templates.FirstOrDefault()?.Id;
                        product.DeliveryDateId = deliveryDates.FirstOrDefault()?.Id;
                        product.TaxCategoryId = taxes.FirstOrDefault()?.Id;
                        product.WarehouseId = warehouses.FirstOrDefault()?.Id;
                        product.UnitId = units.FirstOrDefault().Id;
                        if (!string.IsNullOrEmpty(productid))
                            product.Id = productid;
                    }



                    foreach (var property in manager.GetProperties)
                    {
                        switch (property.PropertyName.ToLower())
                        {
                            case "producttypeid":
                                product.ProductTypeId = property.IntValue;
                                break;
                            case "parentgroupedproductid":
                                var parentgroupedproductid = property.StringValue;
                                if(_productService.GetProductById(parentgroupedproductid)!=null)
                                    product.ParentGroupedProductId = property.StringValue;
                                break;
                            case "visibleindividually":
                                product.VisibleIndividually = property.BooleanValue;
                                break;
                            case "name":
                                product.Name = property.StringValue;
                                break;
                            case "shortdescription":
                                product.ShortDescription = property.StringValue;
                                break;
                            case "fulldescription":
                                product.FullDescription = property.StringValue;
                                break;
                            case "vendorid":
                                var vendorid = property.StringValue;
                                if(_vendorService.GetVendorById(vendorid) != null)
                                    product.VendorId = property.StringValue;
                                break;
                            case "producttemplateid":
                                var templateid = property.StringValue;
                                if(templates.FirstOrDefault(x => x.Id == templateid) != null)
                                    product.ProductTemplateId = property.StringValue;
                                break;
                            case "showonhomepage":
                                product.ShowOnHomePage = property.BooleanValue;
                                break;
                            case "metakeywords":
                                product.MetaKeywords = property.StringValue;
                                break;
                            case "metadescription":
                                product.MetaDescription = property.StringValue;
                                break;
                            case "metatitle":
                                product.MetaTitle = property.StringValue;
                                break;
                            case "allowcustomerreviews":
                                product.AllowCustomerReviews = property.BooleanValue;
                                break;
                            case "published":
                                product.Published = property.BooleanValue;
                                break;
                            case "sku":
                                product.Sku = property.StringValue;
                                break;
                            case "manufacturerpartnumber":
                                product.ManufacturerPartNumber = property.StringValue;
                                break;
                            case "gtin":
                                product.Gtin = property.StringValue;
                                break;
                            case "isgiftcard":
                                product.IsGiftCard = property.BooleanValue;
                                break;
                            case "giftcardtypeid":
                                product.GiftCardTypeId = property.IntValue;
                                break;
                            case "overriddengiftcardamount":
                                product.OverriddenGiftCardAmount = property.DecimalValue;
                                break;
                            case "requireotherproducts":
                                product.RequireOtherProducts = property.BooleanValue;
                                break;
                            case "requiredproductids":
                                product.RequiredProductIds = property.StringValue;
                                break;
                            case "automaticallyaddrequiredproducts":
                                product.AutomaticallyAddRequiredProducts = property.BooleanValue;
                                break;
                            case "isdownload":
                                product.IsDownload = property.BooleanValue;
                                break;
                            case "downloadid":
                                var downloadid = property.StringValue;
                                if (_downloadService.GetDownloadById(downloadid) != null)
                                    product.DownloadId = downloadid;
                                break;
                            case "unlimiteddownloads":
                                product.UnlimitedDownloads = property.BooleanValue;
                                break;
                            case "maxnumberofdownloads":
                                product.MaxNumberOfDownloads = property.IntValue;
                                break;
                            case "downloadactivationtypeid":
                                product.DownloadActivationTypeId = property.IntValue;
                                break;
                            case "hassampledownload":
                                product.HasSampleDownload = property.BooleanValue;
                                break;
                            case "sampledownloadid":
                                var sampledownloadid = property.StringValue;
                                if (_downloadService.GetDownloadById(sampledownloadid) != null)
                                    product.SampleDownloadId = property.StringValue;
                                break;
                            case "hasuseragreement":
                                product.HasUserAgreement = property.BooleanValue;
                                break;
                            case "useragreementtext":
                                product.UserAgreementText = property.StringValue;
                                break;
                            case "isrecurring":
                                product.IsRecurring = property.BooleanValue;
                                break;
                            case "recurringcyclelength":
                                product.RecurringCycleLength = property.IntValue;
                                break;
                            case "recurringcycleperiodid":
                                product.RecurringCyclePeriodId = property.IntValue;
                                break;
                            case "recurringtotalcycles":
                                product.RecurringTotalCycles = property.IntValue;
                                break;
                            case "interval":
                                product.Interval = property.IntValue;
                                break;
                            case "intervalunitId":
                                product.IntervalUnitId = property.IntValue;
                                break;
                            case "isshipenabled":
                                product.IsShipEnabled = property.BooleanValue;
                                break;
                            case "isfreeshipping":
                                product.IsFreeShipping = property.BooleanValue;
                                break;
                            case "shipseparately":
                                product.ShipSeparately = property.BooleanValue;
                                break;
                            case "additionalshippingcharge":
                                product.AdditionalShippingCharge = property.DecimalValue;
                                break;
                            case "deliverydateId":
                                var deliverydateid = property.StringValue;
                                if(deliveryDates.FirstOrDefault(x=>x.Id == deliverydateid) != null)
                                    product.DeliveryDateId = deliverydateid;
                                break;
                            case "istaxexempt":
                                product.IsTaxExempt = property.BooleanValue;
                                break;
                            case "taxcategoryid":
                                var taxcategoryid = property.StringValue;
                                if(taxes.FirstOrDefault(x=>x.Id == taxcategoryid) != null)
                                    product.TaxCategoryId = property.StringValue;
                                break;
                            case "istelecommunicationsorbroadcastingorelectronicservices":
                                product.IsTelecommunicationsOrBroadcastingOrElectronicServices = property.BooleanValue;
                                break;
                            case "manageinventorymethodid":
                                product.ManageInventoryMethodId = property.IntValue;
                                break;
                            case "usemultiplewarehouses":
                                product.UseMultipleWarehouses = property.BooleanValue;
                                break;
                            case "warehouseid":
                                var warehouseid = property.StringValue;
                                if(warehouses.FirstOrDefault(x=>x.Id == warehouseid) != null)
                                    product.WarehouseId = property.StringValue;
                                break;
                            case "stockquantity":
                                product.StockQuantity = property.IntValue;
                                break;
                            case "displaystockavailability":
                                product.DisplayStockAvailability = property.BooleanValue;
                                break;
                            case "displaystockquantity":
                                product.DisplayStockQuantity = property.BooleanValue;
                                break;
                            case "minstockquantity":
                                product.MinStockQuantity = property.IntValue;
                                break;
                            case "lowstockactivityid":
                                product.LowStockActivityId = property.IntValue;
                                break;
                            case "notifyadminforquantitybelow":
                                product.NotifyAdminForQuantityBelow = property.IntValue;
                                break;
                            case "admincomment":
                                product.AdminComment = property.StringValue;
                                break;
                            case "flag":
                                product.Flag = property.StringValue;
                                break;
                            case "backordermodeid":
                                product.BackorderModeId = property.IntValue;
                                break;
                            case "allowbackinstocksubscriptions":
                                product.AllowBackInStockSubscriptions = property.BooleanValue;
                                break;
                            case "orderminimumquantity":
                                product.OrderMinimumQuantity = property.IntValue;
                                break;
                            case "ordermaximumquantity":
                                product.OrderMaximumQuantity = property.IntValue;
                                break;
                            case "allowedquantities":
                                product.AllowedQuantities = property.StringValue;
                                break;
                            case "allowaddingonlyexistingattributecombinations":
                                product.AllowAddingOnlyExistingAttributeCombinations = property.BooleanValue;
                                break;
                            case "disablebuybutton":
                                product.DisableBuyButton = property.BooleanValue;
                                break;
                            case "disablewishlistbutton":
                                product.DisableWishlistButton = property.BooleanValue;
                                break;
                            case "availableforpreorder":
                                product.AvailableForPreOrder = property.BooleanValue;
                                break;
                            case "preorderavailabilitystartdatetimeutc":
                                product.PreOrderAvailabilityStartDateTimeUtc = property.DateTimeNullable;
                                break;
                            case "callforprice":
                                product.CallForPrice = property.BooleanValue;
                                break;
                            case "price":
                                product.Price = property.DecimalValue;
                                break;
                            case "oldprice":
                                product.OldPrice = property.DecimalValue;
                                break;
                            case "catalogprice":
                                product.CatalogPrice = property.DecimalValue;
                                break;
                            case "startprice":
                                product.StartPrice = property.DecimalValue;
                                break;
                            case "productcost":
                                product.ProductCost = property.DecimalValue;
                                break;
                            case "customerentersprice":
                                product.CustomerEntersPrice = property.BooleanValue;
                                break;
                            case "minimumcustomerenteredprice":
                                product.MinimumCustomerEnteredPrice = property.DecimalValue;
                                break;
                            case "maximumcustomerenteredprice":
                                product.MaximumCustomerEnteredPrice = property.DecimalValue;
                                break;
                            case "basepriceenabled":
                                product.BasepriceEnabled = property.BooleanValue;
                                break;
                            case "basepriceamount":
                                product.BasepriceAmount = property.DecimalValue;
                                break;
                            case "basepriceunitid":
                                product.BasepriceUnitId = property.StringValue;
                                break;
                            case "basepricebaseamount":
                                product.BasepriceBaseAmount = property.DecimalValue;
                                break;
                            case "basepricebaseunitid":
                                product.BasepriceBaseUnitId = property.StringValue;
                                break;
                            case "markasnew":
                                product.MarkAsNew = property.BooleanValue;
                                break;
                            case "markasnewstartdatetimeutc":
                                product.MarkAsNewStartDateTimeUtc = property.DateTimeNullable;
                                break;
                            case "markasnewenddatetimeutc":
                                product.MarkAsNewEndDateTimeUtc = property.DateTimeNullable;
                                break;
                            case "unitid":
                                var unitid = property.StringValue;
                                if(units.FirstOrDefault(x=>x.Id == unitid) != null)
                                    product.UnitId = property.StringValue;
                                break;
                            case "weight":
                                product.Weight = property.DecimalValue;
                                break;
                            case "length":
                                product.Length = property.DecimalValue;
                                break;
                            case "width":
                                product.Width = property.DecimalValue;
                                break;
                            case "height":
                                product.Height = property.DecimalValue;
                                break;
                        }
                    }

                    if (isNew && properties.All(p => p.PropertyName.ToLower() != "producttypeid"))
                        product.ProductType = ProductType.SimpleProduct;

                    product.LowStock = product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity;

                    var categoryIds = manager.GetProperty("categoryids") !=null ? manager.GetProperty("categoryids").StringValue : string.Empty;
                    var manufacturerIds = manager.GetProperty("manufacturerids") !=null ? manager.GetProperty("manufacturerids").StringValue : string.Empty;

                    var picture1 = manager.GetProperty("picture1") != null ? manager.GetProperty("picture1").StringValue : string.Empty;
                    var picture2 = manager.GetProperty("picture2") != null ? manager.GetProperty("picture2").StringValue : string.Empty;
                    var picture3 = manager.GetProperty("picture3") != null ? manager.GetProperty("picture3").StringValue : string.Empty;

                    product.UpdatedOnUtc = DateTime.UtcNow;

                    if (isNew)
                    {
                        _productService.InsertProduct(product);
                    }
                    else
                    {
                        _productService.UpdateProduct(product);
                    }

                    //search engine name
                    var seName = manager.GetProperty("sename")!=null ? manager.GetProperty("sename").StringValue : product.Name;
                    _urlRecordService.SaveSlug(product, product.ValidateSeName(seName, product.Name, true), "");
                    var _seName = product.ValidateSeName(seName, product.Name, true);
                    //search engine name
                    _urlRecordService.SaveSlug(product, _seName, "");
                    product.SeName = _seName;
                    _productService.UpdateProduct(product);
                    //category mappings
                    if (!String.IsNullOrEmpty(categoryIds))
                    {
                        foreach (var id in categoryIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                        {
                            if (product.ProductCategories.FirstOrDefault(x => x.CategoryId == id) == null)
                            {
                                //ensure that category exists
                                var category = _categoryService.GetCategoryById(id);
                                if (category != null)
                                {
                                    var productCategory = new ProductCategory
                                    {
                                        ProductId = product.Id,
                                        CategoryId = category.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _categoryService.InsertProductCategory(productCategory);
                                }
                            }
                        }
                    }

                    //manufacturer mappings
                    if (!String.IsNullOrEmpty(manufacturerIds))
                    {
                        foreach (var id in manufacturerIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                        {
                            if (product.ProductManufacturers.FirstOrDefault(x => x.ManufacturerId == id) == null)
                            {
                                //ensure that manufacturer exists
                                var manufacturer = _manufacturerService.GetManufacturerById(id);
                                if (manufacturer != null)
                                {
                                    var productManufacturer = new ProductManufacturer
                                    {
                                        ProductId = product.Id,
                                        ManufacturerId = manufacturer.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _manufacturerService.InsertProductManufacturer(productManufacturer);
                                }
                            }
                        }
                    }

                    //pictures
                    foreach (var picturePath in new[] { picture1, picture2, picture3 })
                    {
                        if (String.IsNullOrEmpty(picturePath))
                            continue;
                        if (!picturePath.ToLower().StartsWith(("http".ToLower())))
                        {
                            var mimeType = GetMimeTypeFromFilePath(picturePath);
                            var newPictureBinary = File.ReadAllBytes(picturePath);
                            var pictureAlreadyExists = false;
                            if (!isNew)
                            {
                                //compare with existing product pictures
                                var existingPictures = product.ProductPictures;
                                foreach (var existingPicture in existingPictures)
                                {
                                    var pp = _pictureService.GetPictureById(existingPicture.PictureId);
                                    var existingBinary = _pictureService.LoadPictureBinary(pp);
                                    //picture binary after validation (like in database)
                                    var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                                    if (existingBinary.SequenceEqual(validatedPictureBinary) || existingBinary.SequenceEqual(newPictureBinary))
                                    {
                                        //the same picture content
                                        pictureAlreadyExists = true;
                                        break;
                                    }
                                }
                            }

                            if (!pictureAlreadyExists)
                            {
                                var picture = _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(product.Name));
                                var productPicture = new ProductPicture
                                {
                                    PictureId = picture.Id,
                                    ProductId = product.Id,
                                    DisplayOrder = 1,
                                };
                                _productService.InsertProductPicture(productPicture);
                            }
                        }
                        else
                        {
                            byte[] fileBinary = DownloadUrl.DownloadFile(picturePath).Result;
                            if (fileBinary!=null)
                            {
                                var mimeType = GetMimeTypeFromFilePath(picturePath);
                                var picture = _pictureService.InsertPicture(fileBinary, mimeType, _pictureService.GetPictureSeName(product.Name));
                                var productPicture = new ProductPicture
                                {
                                    PictureId = picture.Id,
                                    ProductId = product.Id,
                                    DisplayOrder = 1,
                                };
                                _productService.InsertProductPicture(productPicture);
                            }
                        }
                    }

                    //next product
                    iRow++;
                }
            }
        }

        /// <summary>
        /// Import newsletter subscribers from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported subscribers</returns>
        public virtual int ImportNewsletterSubscribersFromTxt(Stream stream)
        {
            int count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        continue;
                    string[] tmp = line.Split(',');

                    var email = "";
                    bool isActive = true;
                    string storeId = _storeContext.CurrentStore.Id;
                    //parse
                    if (tmp.Length == 1)
                    {
                        //"email" only
                        email = tmp[0].Trim();
                    }
                    else if (tmp.Length == 2)
                    {
                        //"email" and "active" fields specified
                        email = tmp[0].Trim();
                        isActive = Boolean.Parse(tmp[1].Trim());
                    }
                    else if (tmp.Length == 3)
                    {
                        //"email" and "active" and "storeId" fields specified
                        email = tmp[0].Trim();
                        isActive = Boolean.Parse(tmp[1].Trim());
                        storeId = tmp[2].Trim();
                    }
                    else
                        throw new GrandException("Wrong file format");

                    //import
                    var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, storeId);
                    if (subscription != null)
                    {
                        subscription.Email = email;
                        subscription.Active = isActive;
                        _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
                    }
                    else
                    {
                        subscription = new NewsLetterSubscription
                        {
                            Active = isActive,
                            CreatedOnUtc = DateTime.UtcNow,
                            Email = email,
                            StoreId = storeId,
                            NewsLetterSubscriptionGuid = Guid.NewGuid()
                        };
                        _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
                    }
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import states from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported states</returns>
        public virtual int ImportStatesFromTxt(Stream stream)
        {
            int count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        continue;
                    string[] tmp = line.Split(',');

                    if (tmp.Length != 5)
                        throw new GrandException("Wrong file format");

                    //parse
                    var countryTwoLetterIsoCode = tmp[0].Trim();
                    var name = tmp[1].Trim();
                    var abbreviation = tmp[2].Trim();
                    bool published = Boolean.Parse(tmp[3].Trim());
                    int displayOrder = Int32.Parse(tmp[4].Trim());

                    var country = _countryService.GetCountryByTwoLetterIsoCode(countryTwoLetterIsoCode);
                    if (country == null)
                    {
                        //country cannot be loaded. skip
                        continue;
                    }

                    //import
                    var states = _stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true);
                    var state = states.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (state != null)
                    {
                        state.Abbreviation = abbreviation;
                        state.Published = published;
                        state.DisplayOrder = displayOrder;
                        _stateProvinceService.UpdateStateProvince(state);
                    }
                    else
                    {
                        state = new StateProvince
                        {
                            CountryId = country.Id,
                            Name = name,
                            Abbreviation = abbreviation,
                            Published = published,
                            DisplayOrder = displayOrder,
                        };
                        _stateProvinceService.InsertStateProvince(state);
                    }
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import manufacturers from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual void ImportManufacturerFromXlsx(Stream stream)
        {
            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new GrandException("No worksheet found");

                //property array
                //the columns
                var properties = new List<PropertyByName<Manufacturer>>();
                var poz = 1;
                while (true)
                {
                    try
                    {
                        var cell = worksheet.Cells[1, poz];

                        if (cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()))
                            break;

                        poz += 1;
                        properties.Add(new PropertyByName<Manufacturer>(cell.Value.ToString().ToLower()));
                    }
                    catch
                    {
                        break;
                    }
                }
                var manager = new PropertyManager<Manufacturer>(properties.ToArray());
                var templates = _manufacturerTemplateService.GetAllManufacturerTemplates();

                var iRow = 2;

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                        .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                        .All(cell => cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    manager.ReadFromXlsx(worksheet, iRow);
                    var manufacturerid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;
                    var manufacturer = string.IsNullOrEmpty(manufacturerid) ? null : _manufacturerService.GetManufacturerById(manufacturerid);

                    var isNew = manufacturer == null;

                    manufacturer = manufacturer ?? new Manufacturer();

                    if (isNew)
                    {
                        manufacturer.CreatedOnUtc = DateTime.UtcNow;
                        manufacturer.ManufacturerTemplateId = templates.FirstOrDefault()?.Id;
                        if (!string.IsNullOrEmpty(manufacturerid))
                            manufacturer.Id = manufacturerid;
                    }

                    string sename = string.Empty;
                    string picture = string.Empty;
                    foreach (var property in manager.GetProperties)
                    {
                        switch (property.PropertyName.ToLower())
                        {
                            case "name":
                                manufacturer.Name = property.StringValue;
                                break;
                            case "description":
                                manufacturer.Description = property.StringValue;
                                break;
                            case "manufacturertemplateid":
                                var manufacturerTemplateId = property.StringValue;
                                if(templates.FirstOrDefault(x => x.Id == manufacturerTemplateId) != null)
                                    manufacturer.ManufacturerTemplateId = property.StringValue;
                                break;
                            case "metakeywords":
                                manufacturer.MetaKeywords = property.StringValue;
                                break;
                            case "metadescription":
                                manufacturer.MetaDescription = property.StringValue;
                                break;
                            case "metatitle":
                                manufacturer.MetaTitle = property.StringValue;
                                break;
                            case "sename":
                                sename = property.StringValue;
                                break;
                            case "pagesize":
                                manufacturer.PageSize = property.IntValue > 0 ? property.IntValue : 10;
                                break;
                            case "allowcustomerstoselectpageSize":
                                manufacturer.AllowCustomersToSelectPageSize = property.BooleanValue;
                                break;
                            case "pagesizeoptions":
                                manufacturer.PageSizeOptions = property.StringValue;
                                break;
                            case "priceranges":
                                manufacturer.PriceRanges = property.StringValue;
                                break;
                            case "published":
                                manufacturer.Published = property.BooleanValue;
                                break;
                            case "showonhomepage":
                                manufacturer.ShowOnHomePage = property.BooleanValue;
                                break;
                            case "includeintopmenu":
                                manufacturer.IncludeInTopMenu = property.BooleanValue;
                                break;
                            case "displayorder":
                                manufacturer.DisplayOrder = property.IntValue;
                                break;
                            case "picture":
                                picture = property.StringValue;
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(picture))
                    {
                        var _picture = LoadPicture(picture, manufacturer.Name,
                            isNew ? "" : manufacturer.PictureId);
                        if (_picture != null)
                            manufacturer.PictureId = _picture.Id;
                    }
                    manufacturer.UpdatedOnUtc = DateTime.UtcNow;

                    if (isNew)
                        _manufacturerService.InsertManufacturer(manufacturer);
                    else
                        _manufacturerService.UpdateManufacturer(manufacturer);

                    sename = manufacturer.ValidateSeName(sename, manufacturer.Name, true);
                    manufacturer.SeName = sename;
                    _manufacturerService.UpdateManufacturer(manufacturer);
                    _urlRecordService.SaveSlug(manufacturer, manufacturer.SeName, "");

                    iRow++;
                }
            }
        }

        /// <summary>
        /// Import categories from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual void ImportCategoryFromXlsx(Stream stream)
        {
            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new GrandException("No worksheet found");

                var iRow = 2;

                //property array
                //the columns
                var properties = new List<PropertyByName<Category>>();
                var poz = 1;
                while (true)
                {
                    try
                    {
                        var cell = worksheet.Cells[1, poz];

                        if (cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()))
                            break;

                        poz += 1;
                        properties.Add(new PropertyByName<Category>(cell.Value.ToString().ToLower()));
                    }
                    catch
                    {
                        break;
                    }
                }
                var manager = new PropertyManager<Category>(properties.ToArray());
                var templates = _categoryTemplateService.GetAllCategoryTemplates();

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                        .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                        .All(cell => cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    manager.ReadFromXlsx(worksheet, iRow);

                    var categoryid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;
                    var category = string.IsNullOrEmpty(categoryid) ? null : _categoryService.GetCategoryById(categoryid);

                    var isNew = category == null;

                    category = category ?? new Category();

                    if (isNew)
                    {
                        category.CreatedOnUtc = DateTime.UtcNow;
                        category.CategoryTemplateId = templates.FirstOrDefault()?.Id;
                        if (!string.IsNullOrEmpty(categoryid))
                            category.Id = categoryid;

                    }

                    string sename = string.Empty;
                    string picture = string.Empty;

                    foreach (var property in manager.GetProperties)
                    {
                        switch (property.PropertyName.ToLower())
                        {
                            case "name":
                                category.Name = property.StringValue;
                                break;
                            case "description":
                                category.Description = property.StringValue;
                                break;
                            case "categorytemplateid":
                                var categorytemplateid = property.StringValue;
                                if(templates.FirstOrDefault(x => x.Id == categorytemplateid) != null)
                                    category.CategoryTemplateId = property.StringValue;
                                break;
                            case "metakeywords":
                                category.MetaKeywords = property.StringValue;
                                break;
                            case "metadescription":
                                category.MetaDescription = property.StringValue;
                                break;
                            case "metatitle":
                                category.MetaTitle = property.StringValue;
                                break;
                            case "sename":
                                sename = property.StringValue;
                                break;
                            case "pagesize":
                                category.PageSize = property.IntValue > 0 ? property.IntValue : 10;
                                break;
                            case "allowcustomerstoselectpageSize":
                                category.AllowCustomersToSelectPageSize = property.BooleanValue;
                                break;
                            case "pagesizeoptions":
                                category.PageSizeOptions = property.StringValue;
                                break;
                            case "priceranges":
                                category.PriceRanges = property.StringValue;
                                break;
                            case "published":
                                category.Published = property.BooleanValue;
                                break;
                            case "displayorder":
                                category.DisplayOrder = property.IntValue;
                                break;
                            case "showonhomepage":
                                category.ShowOnHomePage = property.BooleanValue;
                                break;
                            case "includeintopmenu":
                                category.IncludeInTopMenu = property.BooleanValue;
                                break;
                            case "showonsearchbox":
                                category.ShowOnSearchBox = property.BooleanValue;
                                break;
                            case "searchboxdisplayorder":
                                category.SearchBoxDisplayOrder = property.IntValue;
                                break;
                            case "picture":
                                picture = property.StringValue;
                                break;
                            case "flag":
                                category.Flag = property.StringValue;
                                break;
                            case "flagstyle":
                                category.FlagStyle = property.StringValue;
                                break;
                            case "icon":
                                category.Icon = property.StringValue;
                                break;
                            case "parentcategoryid":
                                if(!string.IsNullOrEmpty(property.StringValue) && property.StringValue!="0")
                                    category.ParentCategoryId = property.StringValue;
                                break;

                        }
                    }

                    if(!string.IsNullOrEmpty(picture))
                    {
                        var _picture = LoadPicture(picture, category.Name, isNew ? "" : category.PictureId);
                        if (_picture != null)
                            category.PictureId = _picture.Id;
                    }

                    category.UpdatedOnUtc = DateTime.UtcNow;

                    if (isNew)
                        _categoryService.InsertCategory(category);
                    else
                        _categoryService.UpdateCategory(category);

                    sename = category.ValidateSeName(sename, category.Name, true);
                    category.SeName = sename;
                    _categoryService.UpdateCategory(category);
                    _urlRecordService.SaveSlug(category, sename, "");

                    iRow++;
                }
            }
        }

        #endregion
    }
}