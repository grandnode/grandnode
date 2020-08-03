using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Common;
using Grand.Services.Directory;
using Grand.Services.Events;
using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address service
    /// </summary>
    public partial class AddressService : IAddressService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : address ID
        /// </remarks>
        private const string ADDRESSES_BY_ID_KEY = "Grand.address.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ADDRESSES_PATTERN_KEY = "Grand.address.";

        #endregion

        #region Fields

        private readonly IRepository<Address> _addressRepository;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IMediator _mediator;
        private readonly AddressSettings _addressSettings;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="addressRepository">Address repository</param>
        /// <param name="countryService">Country service</param>
        /// <param name="stateProvinceService">State/province service</param>
        /// <param name="addressAttributeService">Address attribute service</param>
        /// <param name="mediator">Mediator</param>
        /// <param name="addressSettings">Address settings</param>
        public AddressService(ICacheManager cacheManager,
            IRepository<Address> addressRepository,
            ICountryService countryService, 
            IStateProvinceService stateProvinceService,
            IAddressAttributeService addressAttributeService,
            IMediator mediator, 
            AddressSettings addressSettings)
        {
            _cacheManager = cacheManager;
            _addressRepository = addressRepository;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressAttributeService = addressAttributeService;
            _mediator = mediator;
            _addressSettings = addressSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets total number of addresses by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of addresses</returns>
        public virtual async Task<int> GetAddressTotalByCountryId(string countryId)
        {
            if (String.IsNullOrEmpty(countryId))
                return 0;

            var query = from a in _addressRepository.Table
                        where a.CountryId == countryId
                        select a;
            return await query.CountAsync();
        }

        /// <summary>
        /// Gets total number of addresses by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <returns>Number of addresses</returns>
        public virtual async Task<int> GetAddressTotalByStateProvinceId(string stateProvinceId)
        {
            if (String.IsNullOrEmpty(stateProvinceId))
                return 0;

            var query = from a in _addressRepository.Table
                        where a.StateProvinceId == stateProvinceId
                        select a;
            return await query.CountAsync();
        }

        /// <summary>
        /// Gets an address by address identifier
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <returns>Address</returns>
        public virtual async Task<Address> GetAddressByIdSettings(string addressId)
        {
            if (String.IsNullOrEmpty(addressId))
                return null;

            string key = string.Format(ADDRESSES_BY_ID_KEY, addressId);
            return await _cacheManager.GetAsync(key, () => _addressRepository.GetByIdAsync(addressId));
        }

        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        public virtual async Task InsertAddressSettings(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            
            address.CreatedOnUtc = DateTime.UtcNow;
            await _addressRepository.InsertAsync(address);

            //cache
            await _cacheManager.RemoveByPrefix(ADDRESSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(address);
        }

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        public virtual async Task UpdateAddressSettings(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            await _addressRepository.UpdateAsync(address);

            //cache
            await _cacheManager.RemoveByPrefix(ADDRESSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(address);
        }

        /// <summary>
        /// Gets a value indicating whether address is valid (can be saved)
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>Result</returns>
        public virtual async Task<bool> IsAddressValid(Address address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            if (String.IsNullOrWhiteSpace(address.FirstName))
                return false;

            if (String.IsNullOrWhiteSpace(address.LastName))
                return false;

            if (String.IsNullOrWhiteSpace(address.Email))
                return false;

            if (_addressSettings.CompanyEnabled &&
                _addressSettings.CompanyRequired &&
                String.IsNullOrWhiteSpace(address.Company))
                return false;

            if (_addressSettings.VatNumberEnabled &&
                _addressSettings.VatNumberRequired &&
                String.IsNullOrWhiteSpace(address.VatNumber))
                return false;


            if (_addressSettings.StreetAddressEnabled &&
                _addressSettings.StreetAddressRequired &&
                String.IsNullOrWhiteSpace(address.Address1))
                return false;

            if (_addressSettings.StreetAddress2Enabled &&
                _addressSettings.StreetAddress2Required &&
                String.IsNullOrWhiteSpace(address.Address2))
                return false;

            if (_addressSettings.ZipPostalCodeEnabled &&
                _addressSettings.ZipPostalCodeRequired &&
                String.IsNullOrWhiteSpace(address.ZipPostalCode))
                return false;


            if (_addressSettings.CountryEnabled)
            {
                if (String.IsNullOrEmpty(address.CountryId))
                    return false;

                var country = await _countryService.GetCountryById(address.CountryId);
                if (country == null)
                    return false;

                if (_addressSettings.StateProvinceEnabled)
                {
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(country.Id);
                    if (states.Any())
                    {
                        if (String.IsNullOrEmpty(address.StateProvinceId))
                            return false;

                        var state = states.FirstOrDefault(x => x.Id == address.StateProvinceId);
                        if (state == null)
                            return false;
                    }
                }
            }

            if (_addressSettings.CityEnabled &&
                _addressSettings.CityRequired &&
                String.IsNullOrWhiteSpace(address.City))
                return false;

            if (_addressSettings.PhoneEnabled &&
                _addressSettings.PhoneRequired &&
                String.IsNullOrWhiteSpace(address.PhoneNumber))
                return false;

            if (_addressSettings.FaxEnabled &&
                _addressSettings.FaxRequired &&
                String.IsNullOrWhiteSpace(address.FaxNumber))
                return false;

            var attributes = await _addressAttributeService.GetAllAddressAttributes();
            if (attributes.Any(x => x.IsRequired))
                return false;

            return true;
        }
        
        #endregion
    }
}