using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Mvc.Models;
using Grand.Framework.UI.Paging;
using Grand.Core.Infrastructure;
using Grand.Web.Infrastructure.Cache;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Models.Catalog
{
    public partial class CatalogPagingFilteringModel : BasePageableModel
    {
        #region Constructors

        public CatalogPagingFilteringModel()
        {
            this.AvailableSortOptions = new List<SelectListItem>();
            this.AvailableViewModes = new List<SelectListItem>();
            this.PageSizeOptions = new List<SelectListItem>();

            this.PriceRangeFilter = new PriceRangeFilterModel();
            this.SpecificationFilter = new SpecificationFilterModel();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Price range filter model
        /// </summary>
        public PriceRangeFilterModel PriceRangeFilter { get; set; }

        /// <summary>
        /// Specification filter model
        /// </summary>
        public SpecificationFilterModel SpecificationFilter { get; set; }

        public bool AllowProductSorting { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public bool AllowProductViewModeChanging { get; set; }
        public IList<SelectListItem> AvailableViewModes { get; set; }

        public bool AllowCustomersToSelectPageSize { get; set; }
        public IList<SelectListItem> PageSizeOptions { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Product sorting
        /// </summary>
        public string ViewMode { get; set; }
        

        #endregion

        #region Nested classes

        public partial class PriceRangeFilterModel : BaseGrandModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "price";

            #endregion 

            #region Ctor

            public PriceRangeFilterModel()
            {
                this.Items = new List<PriceRangeFilterItem>();
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Gets parsed price ranges
            /// </summary>
            protected virtual IList<PriceRange> GetPriceRangeList(string priceRangesStr)
            {
                var priceRanges = new List<PriceRange>();
                if (string.IsNullOrWhiteSpace(priceRangesStr))
                    return priceRanges;
                try
                {
                    string[] rangeArray = priceRangesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str1 in rangeArray)
                    {
                        string[] fromTo = str1.Trim().Split(new[] { '-' });

                        decimal? from = null;
                        if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                            from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));

                        decimal? to = null;
                        if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                            to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));

                        priceRanges.Add(new PriceRange { From = from, To = to });
                    }
                    return priceRanges;
                }
                catch
                {
                    return priceRanges;
                }
            }

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            #endregion

            #region Methods

            public virtual PriceRange GetSelectedPriceRange(IWebHelper webHelper, string priceRangesStr)
            {
                var range = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (String.IsNullOrEmpty(range))
                    return null;
                string[] fromTo = range.Trim().Split(new [] { '-' });
                if (fromTo.Length == 2)
                {
                    decimal? from = null;
                    if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                    decimal? to = null;
                    if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                        to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));

                    var priceRangeList = GetPriceRangeList(priceRangesStr);
                    foreach (var pr in priceRangeList)
                    {
                        if (pr.From == from && pr.To == to)
                            return pr;
                    }
                }
                return null;
            }

            public virtual void LoadPriceRangeFilters(string priceRangeStr, IWebHelper webHelper, IPriceFormatter priceFormatter)
            {
                var priceRangeList = GetPriceRangeList(priceRangeStr);
                if (priceRangeList.Any())
                {
                    this.Enabled = true;

                    var selectedPriceRange = GetSelectedPriceRange(webHelper, priceRangeStr);

                    this.Items = priceRangeList.ToList().Select(x =>
                    {
                        //from&to
                        var item = new PriceRangeFilterItem();
                        if (x.From.HasValue)
                            item.From = priceFormatter.FormatPrice(x.From.Value, true, false);
                        if (x.To.HasValue)
                            item.To = priceFormatter.FormatPrice(x.To.Value, true, false);
                        string fromQuery = string.Empty;
                        if (x.From.HasValue)
                            fromQuery = x.From.Value.ToString(new CultureInfo("en-US"));
                        string toQuery = string.Empty;
                        if (x.To.HasValue)
                            toQuery = x.To.Value.ToString(new CultureInfo("en-US"));

                        //is selected?
                        if (selectedPriceRange != null
                            && selectedPriceRange.From == x.From
                            && selectedPriceRange.To == x.To)
                            item.Selected = true;

                        //filter URL
                        string url = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM + "=" + fromQuery + "-" + toQuery, null);
                        url = ExcludeQueryStringParams(url, webHelper);
                        item.FilterUrl = url;


                        return item;
                    }).ToList();

                    if (selectedPriceRange != null)
                    {
                        //remove filter URL
                        string url = webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM);
                        url = ExcludeQueryStringParams(url, webHelper);
                        this.RemoveFilterUrl = url;
                    }
                }
                else
                {
                    this.Enabled = false;
                }
            }
            
            #endregion

            #region Properties
            public bool Enabled { get; set; }
            public IList<PriceRangeFilterItem> Items { get; set; }
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        public partial class PriceRangeFilterItem : BaseGrandModel
        {
            public string From { get; set; }
            public string To { get; set; }
            public string FilterUrl { get; set; }
            public bool Selected { get; set; }
        }

        public partial class SpecificationFilterModel : BaseGrandModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "specs";

            #endregion

            #region Ctor

            public SpecificationFilterModel()
            {
                this.AlreadyFilteredItems = new List<SpecificationFilterItem>();
                this.NotFilteredItems = new List<SpecificationFilterItem>();
            }

            #endregion

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.RemoveQueryString(url, exclude);
                return url;
            }

            protected virtual string GenerateFilteredSpecQueryParam(IList<string> optionIds)
            {
                if (optionIds == null)
                    return "";

                string result = string.Join(",", optionIds);
                return result;
            }

            #endregion

            #region Methods

            public virtual List<string> GetAlreadyFilteredSpecOptionIds(IWebHelper webHelper)
            {
                var result = new List<string>();

                var alreadyFilteredSpecsStr = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (String.IsNullOrWhiteSpace(alreadyFilteredSpecsStr))
                    return result;

                foreach (var spec in alreadyFilteredSpecsStr.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //string specId;
                    //int.TryParse(spec.Trim(), out specId);
                    if (!result.Contains(spec))
                        result.Add(spec);
                }
                return result;
            }

            public virtual void PrepareSpecsFilters(IList<string> alreadyFilteredSpecOptionIds,
                IList<string> filterableSpecificationAttributeOptionIds,
                ISpecificationAttributeService specificationAttributeService,
                IWebHelper webHelper, IWorkContext workContext, ICacheManager cacheManager)
            {
                Enabled = false;
                var optionIds = filterableSpecificationAttributeOptionIds != null
                    ? string.Join(",", filterableSpecificationAttributeOptionIds.Union(alreadyFilteredSpecOptionIds)) : string.Empty;
                var cacheKey = string.Format(ModelCacheEventConsumer.SPECS_FILTER_MODEL_KEY, optionIds, workContext.WorkingLanguage.Id);

                var allFilters = cacheManager.Get(cacheKey, () =>
                {
                    var _allFilters = new List<SpecificationAttributeOptionFilter>();
                    foreach (var sao in filterableSpecificationAttributeOptionIds.Union(alreadyFilteredSpecOptionIds))
                    {
                        var sa = specificationAttributeService.GetSpecificationAttributeByOptionId(sao);
                        if (sa != null)
                        {
                            _allFilters.Add(new SpecificationAttributeOptionFilter
                            {
                                SpecificationAttributeId = sa.Id,
                                SpecificationAttributeName = sa.GetLocalized(x => x.Name, workContext.WorkingLanguage.Id),
                                SpecificationAttributeDisplayOrder = sa.DisplayOrder,
                                SpecificationAttributeOptionId = sao,
                                SpecificationAttributeOptionName = sa.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == sao).GetLocalized(x => x.Name, workContext.WorkingLanguage.Id),
                                SpecificationAttributeOptionDisplayOrder = sa.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == sao).DisplayOrder,
                                SpecificationAttributeOptionColorRgb = sa.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == sao).ColorSquaresRgb,
                            });
                        }
                    }
                    return _allFilters.ToList();
                });
                if (!allFilters.Any())
                    return;

                //sort loaded options
                allFilters = allFilters.OrderBy(saof => saof.SpecificationAttributeDisplayOrder)
                    .ThenBy(saof => saof.SpecificationAttributeName)
                    .ThenBy(saof => saof.SpecificationAttributeOptionDisplayOrder)
                    .ThenBy(saof => saof.SpecificationAttributeOptionName).ToList();

                //prepare the model properties
                Enabled = true;
                var removeFilterUrl = webHelper.RemoveQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM);
                RemoveFilterUrl = ExcludeQueryStringParams(removeFilterUrl, webHelper);

                //get already filtered specification options
                var alreadyFilteredOptions = allFilters.Where(x => alreadyFilteredSpecOptionIds.Contains(x.SpecificationAttributeOptionId));
                AlreadyFilteredItems = alreadyFilteredOptions.Select(x =>
                    new SpecificationFilterItem
                    {
                        SpecificationAttributeName = x.SpecificationAttributeName,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                        SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb
                    }).ToList();

                //get not filtered specification options
                NotFilteredItems = allFilters.Except(alreadyFilteredOptions).Select(x =>
                {
                    //filter URL
                    var alreadyFiltered = alreadyFilteredSpecOptionIds.Concat(new List<string> { x.SpecificationAttributeOptionId });
                    var queryString = string.Format("{0}={1}", QUERYSTRINGPARAM, GenerateFilteredSpecQueryParam(alreadyFiltered.ToList()));
                    var filterUrl = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), queryString, null);

                    return new SpecificationFilterItem()
                    {
                        SpecificationAttributeName = x.SpecificationAttributeName,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                        SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper)
                    };
                }).ToList();
            }

            #endregion

            #region Properties
            public bool Enabled { get; set; }
            public IList<SpecificationFilterItem> AlreadyFilteredItems { get; set; }
            public IList<SpecificationFilterItem> NotFilteredItems { get; set; }
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        public partial class SpecificationFilterItem : BaseGrandModel
        {
            public string SpecificationAttributeName { get; set; }
            public string SpecificationAttributeOptionName { get; set; }
            public string SpecificationAttributeOptionColorRgb { get; set; }
            public string FilterUrl { get; set; }
        }

        #endregion
    }
}