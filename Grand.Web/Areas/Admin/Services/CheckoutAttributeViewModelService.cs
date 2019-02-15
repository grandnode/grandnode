using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public class CheckoutAttributeViewModelService : ICheckoutAttributeViewModelService
    {
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;


        public CheckoutAttributeViewModelService(ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ILocalizationService localizationService,
            ITaxCategoryService taxCategoryService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            CurrencySettings currencySettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService
            )
        {
            _checkoutAttributeService = checkoutAttributeService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _localizationService = localizationService;
            _taxCategoryService = taxCategoryService;
            _workContext = workContext;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _currencySettings = currencySettings;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _storeService = storeService;
            _customerService = customerService;
        }

        #region Utilities

        public virtual void PrepareTaxCategories(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = checkoutAttribute != null && !excludeProperties && tc.Id == checkoutAttribute.TaxCategoryId });
        }

        public virtual void PrepareConditionAttributes(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //currenty any checkout attribute can have condition.
            model.ConditionAllowed = true;

            if (checkoutAttribute == null)
                return;

            var selectedAttribute = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttribute.ConditionAttributeXml).FirstOrDefault();
            var selectedValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttribute.ConditionAttributeXml);

            model.ConditionModel = new ConditionModel()
            {
                EnableCondition = !string.IsNullOrEmpty(checkoutAttribute.ConditionAttributeXml),
                SelectedAttributeId = selectedAttribute != null ? selectedAttribute.Id : "",
                ConditionAttributes = _checkoutAttributeService.GetAllCheckoutAttributes()
                    //ignore this attribute and non-combinable attributes
                    .Where(x => x.Id != checkoutAttribute.Id && x.CanBeUsedAsCondition())
                    .Select(x =>
                        new AttributeConditionModel()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AttributeControlType = x.AttributeControlType,
                            Values = x.CheckoutAttributeValues
                                .Select(v => new SelectListItem()
                                {
                                    Text = v.Name,
                                    Value = v.Id.ToString(),
                                    Selected = selectedAttribute != null && selectedAttribute.Id == x.Id && selectedValues.Any(sv => sv.Id == v.Id)
                                }).ToList()
                        }).ToList()
            };
        }

        protected virtual void SaveConditionAttributes(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
        {
            string attributesXml = null;
            if (model.ConditionModel.EnableCondition)
            {
                var attribute = _checkoutAttributeService.GetCheckoutAttributeById(model.ConditionModel.SelectedAttributeId);
                if (attribute != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValue = selectedAttribute != null ? selectedAttribute.SelectedValueId : null;
                                if (!String.IsNullOrEmpty(selectedValue))
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, selectedValue);
                                else
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValues = selectedAttribute != null ? selectedAttribute.Values.Where(x => x.Selected).Select(x => x.Value) : null;
                                if (selectedValues.Any())
                                    foreach (var value in selectedValues)
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, value);
                                else
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are not supported as conditions
                            break;
                    }
                }
            }
            checkoutAttribute.ConditionAttributeXml = attributesXml;
        }


        #endregion

        public virtual IEnumerable<CheckoutAttributeModel> PrepareCheckoutAttributeListModel()
        {
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes();
            return checkoutAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                });
        }
        public virtual IEnumerable<CheckoutAttributeValueModel> PrepareCheckoutAttributeValuesModel(string checkoutAttributeId)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var values = checkoutAttribute.CheckoutAttributeValues;
            return values.Select(x => new CheckoutAttributeValueModel
            {
                Id = x.Id,
                CheckoutAttributeId = x.CheckoutAttributeId,
                Name = checkoutAttribute.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                ColorSquaresRgb = x.ColorSquaresRgb,
                PriceAdjustment = x.PriceAdjustment,
                WeightAdjustment = x.WeightAdjustment,
                IsPreSelected = x.IsPreSelected,
                DisplayOrder = x.DisplayOrder,
            });
        }
        public virtual CheckoutAttributeModel PrepareCheckoutAttributeModel()
        {
            var model = new CheckoutAttributeModel();
            //tax categories
            PrepareTaxCategories(model, null, true);
            //condition
            PrepareConditionAttributes(model, null);
            return model;
        }

        public virtual CheckoutAttributeValueModel PrepareCheckoutAttributeValueModel(string checkoutAttributeId)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var model = new CheckoutAttributeValueModel();
            model.CheckoutAttributeId = checkoutAttributeId;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            //color squares
            model.DisplayColorSquaresRgb = checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";
            return model;
        }
        public virtual CheckoutAttributeValueModel PrepareCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue)
        {
            var model = checkoutAttributeValue.ToModel();
            model.DisplayColorSquaresRgb = checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            return model;
        }
        public virtual CheckoutAttribute InsertCheckoutAttributeModel(CheckoutAttributeModel model)
        {
            var checkoutAttribute = model.ToEntity();
            _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);

            //activity log
            _customerActivityService.InsertActivity("AddNewCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewCheckoutAttribute"), checkoutAttribute.Name);
            return checkoutAttribute;
        }
        public virtual CheckoutAttribute UpdateCheckoutAttributeModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
        {
            checkoutAttribute = model.ToEntity(checkoutAttribute);
            SaveConditionAttributes(checkoutAttribute, model);
            _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

            //activity log
            _customerActivityService.InsertActivity("EditCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.EditCheckoutAttribute"), checkoutAttribute.Name);
            return checkoutAttribute;
        }

        public virtual CheckoutAttributeValue InsertCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValueModel model)
        {
            var cav = model.ToEntity();
            checkoutAttribute.CheckoutAttributeValues.Add(cav);
            _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);
            return cav;
        }

        public virtual CheckoutAttributeValue UpdateCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model)
        {
            checkoutAttributeValue = model.ToEntity(checkoutAttributeValue);
            _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);
            return checkoutAttributeValue;
        }
    }
}
