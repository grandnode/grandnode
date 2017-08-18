using System;
using System.IO;
using System.Linq;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Messages;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Seo;
using OfficeOpenXml;
using MongoDB.Bson;
using Grand.Services.ExportImport.Help;
using Grand.Core.Domain.Media;
using System.Collections.Generic;
using Microsoft.AspNetCore.StaticFiles;

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
            IStateProvinceService stateProvinceService)
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
        }

        #endregion

        #region Utilities

        protected virtual int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            if (columnName == null)
                throw new ArgumentNullException("columnName");

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }

        protected virtual string ConvertColumnToString(object columnValue)
        {
            if (columnValue == null)
                return null;

            return Convert.ToString(columnValue);
        }

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
                        product.CreatedOnUtc = DateTime.UtcNow;


                    foreach (var property in manager.GetProperties)
                    {
                        switch (property.PropertyName.ToLower())
                        {
                            case "producttypeid":
                                product.ProductTypeId = property.IntValue;
                                break;
                            case "parentgroupedproductid":
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
                                product.VendorId = property.StringValue;
                                break;
                            case "producttemplateid":
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
                                product.DownloadId = property.StringValue;
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
                            case "isrental":
                                product.IsRental = property.BooleanValue;
                                break;
                            case "rentalpricelength":
                                product.RentalPriceLength = property.IntValue;
                                break;
                            case "rentalpriceperiodid":
                                product.RentalPricePeriodId = property.IntValue;
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
                                product.DeliveryDateId = property.StringValue;
                                break;
                            case "istaxexempt":
                                product.IsTaxExempt = property.BooleanValue;
                                break;
                            case "taxcategoryid":
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

                    //update "HasTierPrices" and "HasDiscountsApplied" properties
                    _productService.UpdateHasTierPricesProperty(product.Id);

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
            //property array
            var properties = new[]
            {
                new PropertyByName<Manufacturer>("Id"),
                new PropertyByName<Manufacturer>("Name"),
                new PropertyByName<Manufacturer>("Description"),
                new PropertyByName<Manufacturer>("ManufacturerTemplateId"),
                new PropertyByName<Manufacturer>("MetaKeywords"),
                new PropertyByName<Manufacturer>("MetaDescription"),
                new PropertyByName<Manufacturer>("MetaTitle"),
                new PropertyByName<Manufacturer>("SeName"),
                new PropertyByName<Manufacturer>("Picture"),
                new PropertyByName<Manufacturer>("PageSize"),
                new PropertyByName<Manufacturer>("AllowCustomersToSelectPageSize"),
                new PropertyByName<Manufacturer>("PageSizeOptions"),
                new PropertyByName<Manufacturer>("PriceRanges"),
                new PropertyByName<Manufacturer>("Published"),
                new PropertyByName<Manufacturer>("DisplayOrder")
            };

            var manager = new PropertyManager<Manufacturer>(properties);

            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new GrandException("No worksheet found");

                var iRow = 2;

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                        .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                        .All(cell => cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    manager.ReadFromXlsx(worksheet, iRow);

                    var manufacturer = _manufacturerService.GetManufacturerById(manager.GetProperty("Id").StringValue);

                    var isNew = manufacturer == null;

                    manufacturer = manufacturer ?? new Manufacturer();

                    if (isNew)
                        manufacturer.CreatedOnUtc = DateTime.UtcNow;

                    manufacturer.Name = manager.GetProperty("Name").StringValue;
                    manufacturer.Description = manager.GetProperty("Description").StringValue;
                    manufacturer.ManufacturerTemplateId = manager.GetProperty("ManufacturerTemplateId").StringValue;
                    manufacturer.MetaKeywords = manager.GetProperty("MetaKeywords").StringValue;
                    manufacturer.MetaDescription = manager.GetProperty("MetaDescription").StringValue;
                    manufacturer.MetaTitle = manager.GetProperty("MetaTitle").StringValue;
                    var _seName = manager.GetProperty("SeName").StringValue;
                    var picture = LoadPicture(manager.GetProperty("Picture").StringValue, manufacturer.Name,
                        isNew ? "" : manufacturer.PictureId);
                    manufacturer.PageSize = manager.GetProperty("PageSize").IntValue;
                    manufacturer.AllowCustomersToSelectPageSize = manager.GetProperty("AllowCustomersToSelectPageSize").BooleanValue;
                    manufacturer.PageSizeOptions = manager.GetProperty("PageSizeOptions").StringValue;
                    manufacturer.PriceRanges = manager.GetProperty("PriceRanges").StringValue;
                    manufacturer.Published = manager.GetProperty("Published").BooleanValue;
                    manufacturer.DisplayOrder = manager.GetProperty("DisplayOrder").IntValue;

                    if (picture != null)
                        manufacturer.PictureId = picture.Id;

                    manufacturer.UpdatedOnUtc = DateTime.UtcNow;

                    if (isNew)
                        _manufacturerService.InsertManufacturer(manufacturer);
                    else
                        _manufacturerService.UpdateManufacturer(manufacturer);

                    _seName = manufacturer.ValidateSeName(_seName, manufacturer.Name, true);
                    manufacturer.SeName = _seName;
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
            var properties = new[]
            {
                new PropertyByName<Category>("Id"),
                new PropertyByName<Category>("Name"),
                new PropertyByName<Category>("Description"),
                new PropertyByName<Category>("CategoryTemplateId"),
                new PropertyByName<Category>("MetaKeywords"),
                new PropertyByName<Category>("MetaDescription"),
                new PropertyByName<Category>("MetaTitle"),
                new PropertyByName<Category>("SeName"),
                new PropertyByName<Category>("ParentCategoryId"),
                new PropertyByName<Category>("Picture"),
                new PropertyByName<Category>("PageSize"),
                new PropertyByName<Category>("AllowCustomersToSelectPageSize"),
                new PropertyByName<Category>("PageSizeOptions"),
                new PropertyByName<Category>("PriceRanges"),
                new PropertyByName<Category>("ShowOnHomePage"),
                new PropertyByName<Category>("IncludeInTopMenu"),
                new PropertyByName<Category>("Published"),
                new PropertyByName<Category>("DisplayOrder")
            };

            var manager = new PropertyManager<Category>(properties);

            using (var xlPackage = new ExcelPackage(stream))
            {
                // get the first worksheet in the workbook
                var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new GrandException("No worksheet found");

                var iRow = 2;

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetProperties
                        .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                        .All(cell => cell == null || cell.Value == null || String.IsNullOrEmpty(cell.Value.ToString()));

                    if (allColumnsAreEmpty)
                        break;

                    manager.ReadFromXlsx(worksheet, iRow);

                    var category = _categoryService.GetCategoryById(manager.GetProperty("Id").StringValue);

                    var isNew = category == null;

                    category = category ?? new Category();

                    if (isNew)
                        category.CreatedOnUtc = DateTime.UtcNow;

                    category.Name = manager.GetProperty("Name").StringValue;
                    category.Description = manager.GetProperty("Description").StringValue;
                    category.CategoryTemplateId = manager.GetProperty("CategoryTemplateId").StringValue;
                    category.MetaKeywords = manager.GetProperty("MetaKeywords").StringValue;
                    category.MetaDescription = manager.GetProperty("MetaDescription").StringValue;
                    category.MetaTitle = manager.GetProperty("MetaTitle").StringValue;
                    var _seName = manager.GetProperty("SeName").StringValue;
                    category.ParentCategoryId = manager.GetProperty("ParentCategoryId").StringValue;
                    var picture = LoadPicture(manager.GetProperty("Picture").StringValue, category.Name, isNew ? "" : category.PictureId);
                    category.PageSize = manager.GetProperty("PageSize").IntValue;
                    category.AllowCustomersToSelectPageSize = manager.GetProperty("AllowCustomersToSelectPageSize").BooleanValue;
                    category.PageSizeOptions = manager.GetProperty("PageSizeOptions").StringValue;
                    category.PriceRanges = manager.GetProperty("PriceRanges").StringValue;
                    category.ShowOnHomePage = manager.GetProperty("ShowOnHomePage").BooleanValue;
                    category.IncludeInTopMenu = manager.GetProperty("IncludeInTopMenu").BooleanValue;
                    category.Published = manager.GetProperty("Published").BooleanValue;
                    category.DisplayOrder = manager.GetProperty("DisplayOrder").IntValue;

                    if (picture != null)
                        category.PictureId = picture.Id;

                    category.UpdatedOnUtc = DateTime.UtcNow;

                    if (isNew)
                        _categoryService.InsertCategory(category);
                    else
                        _categoryService.UpdateCategory(category);

                    _seName = category.ValidateSeName(_seName, category.Name, true);
                    category.SeName = _seName;
                    _categoryService.UpdateCategory(category);

                    _urlRecordService.SaveSlug(category, _seName, "");


                    iRow++;
                }
            }
        }

        #endregion
    }
}