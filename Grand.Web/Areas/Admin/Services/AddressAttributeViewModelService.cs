using Grand.Core;
using Grand.Domain.Common;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public virtual async Task<(IEnumerable<AddressAttributeModel> addressAttributes, int totalCount)> PrepareAddressAttributes()
        {
            var addressAttributes = await _addressAttributeService.GetAllAddressAttributes();
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
        public virtual async Task<AddressAttribute> InsertAddressAttributeModel(AddressAttributeModel model)
        {
            var addressAttribute = model.ToEntity();
            await _addressAttributeService.InsertAddressAttribute(addressAttribute);

            return addressAttribute;
        }
        public virtual async Task<AddressAttribute> UpdateAddressAttributeModel(AddressAttributeModel model, AddressAttribute addressAttribute)
        {
            addressAttribute = model.ToEntity(addressAttribute);
            await _addressAttributeService.UpdateAddressAttribute(addressAttribute);
            return addressAttribute;
        }

        public virtual async Task<(IEnumerable<AddressAttributeValueModel> addressAttributeValues, int totalCount)> PrepareAddressAttributeValues(string addressAttributeId)
        {
            var values = (await _addressAttributeService.GetAddressAttributeById(addressAttributeId)).AddressAttributeValues;
            return (values.Select(x => x.ToModel()), values.Count());
        }

        public virtual AddressAttributeValueModel PrepareAddressAttributeValueModel(string addressAttributeId)
        {
            var model = new AddressAttributeValueModel();
            model.AddressAttributeId = addressAttributeId;
            return model;
        }

        public virtual async Task<AddressAttributeValue> InsertAddressAttributeValueModel(AddressAttributeValueModel model)
        {
            var addressAttributeValue = model.ToEntity();
            await _addressAttributeService.InsertAddressAttributeValue(addressAttributeValue);
            return addressAttributeValue;
        }
        public virtual AddressAttributeValueModel PrepareAddressAttributeValueModel(AddressAttributeValue addressAttributeValue)
        {
            var model = addressAttributeValue.ToModel();
            return model;
        }

        public virtual async Task<AddressAttributeValue> UpdateAddressAttributeValueModel(AddressAttributeValueModel model, AddressAttributeValue addressAttributeValue)
        {
            addressAttributeValue = model.ToEntity(addressAttributeValue);
            await _addressAttributeService.UpdateAddressAttributeValue(addressAttributeValue);
            return addressAttributeValue;
        }

    }
}
