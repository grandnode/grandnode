using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class AddressAttributeViewModelService : IAddressAttributeViewModelService
    {
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        public AddressAttributeViewModelService(IAddressAttributeService addressAttributeService,
            ILocalizationService localizationService, IWorkContext workContext)
        {
            _addressAttributeService = addressAttributeService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        public virtual (IEnumerable<AddressAttributeModel> addressAttributes, int totalCount) PrepareAddressAttributes()
        {
            var addressAttributes = _addressAttributeService.GetAllAddressAttributes();
            return (addressAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                }), addressAttributes.Count());
        }

        public virtual AddressAttributeModel PrepareAddressAttributeModel()
        {
            var model = new AddressAttributeModel();
            return model;
        }
        public virtual AddressAttributeModel PrepareAddressAttributeModel(AddressAttribute addressAttribute)
        {
            var model = addressAttribute.ToModel();
            return model;
        }
        public virtual AddressAttribute InsertAddressAttributeModel(AddressAttributeModel model)
        {
            var addressAttribute = model.ToEntity();
            _addressAttributeService.InsertAddressAttribute(addressAttribute);

            return addressAttribute;
        }
        public virtual AddressAttribute UpdateAddressAttributeModel(AddressAttributeModel model, AddressAttribute addressAttribute)
        {
            addressAttribute = model.ToEntity(addressAttribute);
            _addressAttributeService.UpdateAddressAttribute(addressAttribute);
            return addressAttribute;
        }

        public virtual (IEnumerable<AddressAttributeValueModel> addressAttributeValues, int totalCount) PrepareAddressAttributeValues(string addressAttributeId)
        {
            var values = _addressAttributeService.GetAddressAttributeById(addressAttributeId).AddressAttributeValues;
            return (values.Select(x => x.ToModel()), values.Count());
        }

        public virtual AddressAttributeValueModel PrepareAddressAttributeValueModel(string addressAttributeId)
        {
            var model = new AddressAttributeValueModel();
            model.AddressAttributeId = addressAttributeId;
            return model;
        }

        public virtual AddressAttributeValue InsertAddressAttributeValueModel(AddressAttributeValueModel model)
        {
            var addressAttributeValue = model.ToEntity();
            _addressAttributeService.InsertAddressAttributeValue(addressAttributeValue);
            return addressAttributeValue;
        }
        public virtual AddressAttributeValueModel PrepareAddressAttributeValueModel(AddressAttributeValue addressAttributeValue)
        {
            var model = addressAttributeValue.ToModel();
            return model;
        }

        public virtual AddressAttributeValue UpdateAddressAttributeValueModel(AddressAttributeValueModel model, AddressAttributeValue addressAttributeValue)
        {
            addressAttributeValue = model.ToEntity(addressAttributeValue);
            _addressAttributeService.UpdateAddressAttributeValue(addressAttributeValue);
            return addressAttributeValue;
        }

    }
}
