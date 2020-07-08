using Grand.Domain.Common;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IAddressService
    {

        /// <summary>
        /// Gets total number of addresses by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of addresses</returns>
        Task<int> GetAddressTotalByCountryId(string countryId);

        /// <summary>
        /// Gets total number of addresses by state/province identifier
        /// </summary>
        /// <param name="stateProvinceId">State/province identifier</param>
        /// <returns>Number of addresses</returns>
        Task<int> GetAddressTotalByStateProvinceId(string stateProvinceId);

        /// <summary>
        /// Gets an address by address identifier
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <returns>Address</returns>
        Task<Address> GetAddressByIdSettings(string addressId);

        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        Task InsertAddressSettings(Address address);

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        Task UpdateAddressSettings(Address address);

        /// <summary>
        /// Gets a value indicating whether address is valid (can be saved)
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>Result</returns>
        Task<bool> IsAddressValid(Address address);
    }
}