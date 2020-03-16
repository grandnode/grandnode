using Grand.Core.Domain.Common;
using Grand.Services.Directory;
using Grand.Services.Stores;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Interfaces;
using Grand.Web.Models.Common;
using Grand.Web.Models.Customer;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetAddressListHandler : IRequestHandler<GetAddressList, CustomerAddressListModel>
    {
        private readonly ICountryService _countryService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAddressViewModelService _addressViewModelService;

        public GetAddressListHandler(ICountryService countryService, 
            IStoreMappingService storeMappingService, 
            IAddressViewModelService addressViewModelService)
        {
            _countryService = countryService;
            _storeMappingService = storeMappingService;
            _addressViewModelService = addressViewModelService;
        }

        public async Task<CustomerAddressListModel> Handle(GetAddressList request, CancellationToken cancellationToken)
        {
            var model = new CustomerAddressListModel();
            var addresses = new List<Address>();
            foreach (var item in request.Customer.Addresses)
            {
                if (string.IsNullOrEmpty(item.CountryId))
                {
                    addresses.Add(item);
                    continue;
                }
                var country = await _countryService.GetCountryById(item.CountryId);
                if (country != null || _storeMappingService.Authorize(country))
                {
                    addresses.Add(item);
                    continue;
                }
            }

            foreach (var address in addresses)
            {
                var countries = await _countryService.GetAllCountries(request.Language.Id);
                var addressModel = new AddressModel();
                await _addressViewModelService.PrepareModel(model: addressModel,
                    address: address,
                    excludeProperties: false,
                    loadCountries: () => countries);
                model.Addresses.Add(addressModel);
            }

            return model;
        }
    }
}
