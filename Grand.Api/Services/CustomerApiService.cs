using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Customers;
using Grand.Core.Data;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public partial class CustomerApiService : ICustomerApiService
    {
        private readonly IMongoDBContext _mongoDBContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        private readonly IMongoCollection<VendorDto> _vendor;

        public CustomerApiService(IMongoDBContext mongoDBContext, ICustomerService customerService, IGenericAttributeService genericAttributeService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService)
        {
            _mongoDBContext = mongoDBContext;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _vendor = _mongoDBContext.Database().GetCollection<VendorDto>(typeof(Core.Domain.Vendors.Vendor).Name);
        }

        protected async Task SaveCustomerAttributes(CustomerDto model, Customer customer)
        {
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, model.VatNumberStatusId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
            await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);
        }

        protected async Task SaveCustomerRoles(CustomerDto model, Customer customer)
        {
            var insertRoles = model.CustomerRoles.Except(customer.CustomerRoles.Select(x => x.Id)).ToList();
            foreach (var item in insertRoles)
            {
                var role = await _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Add(role);
                    role.CustomerId = customer.Id;
                    await _customerService.InsertCustomerRoleInCustomer(role);
                }
            }
            var deleteRoles = customer.CustomerRoles.Select(x => x.Id).Except(model.CustomerRoles).ToList();
            foreach (var item in deleteRoles)
            {
                var role = await _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Remove(customer.CustomerRoles.FirstOrDefault(x => x.Id == role.Id));
                    role.CustomerId = customer.Id;
                    await _customerService.DeleteCustomerRoleInCustomer(role);
                }
            }
        }

        public virtual async Task<CustomerDto> GetByEmail(string email)
        {
            return (await _customerService.GetCustomerByEmail(email)).ToModel();
        }

        public virtual async Task<CustomerDto> InsertOrUpdateCustomer(CustomerDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = await InsertCustomer(model);
            else
                model = await UpdateCustomer(model);

            return model;
        }
        public virtual async Task<CustomerDto> InsertCustomer(CustomerDto model)
        {
            var customer = model.ToEntity();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            if (string.IsNullOrEmpty(customer.Username))
                customer.Username = customer.Email;

            await _customerService.InsertCustomer(customer);
            await SaveCustomerAttributes(model, customer);
            await SaveCustomerRoles(model, customer);

            //activity log
            await _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);
            return customer.ToModel();
        }

        public virtual async Task<CustomerDto> UpdateCustomer(CustomerDto model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            customer = model.ToEntity(customer);
            await _customerService.UpdateCustomer(customer);
            await SaveCustomerAttributes(model, customer);
            await SaveCustomerRoles(model, customer);

            //activity log
            await _customerActivityService.InsertActivity("EditCustomer", customer.Id, _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

            return customer.ToModel();
        }

        public virtual async Task DeleteCustomer(CustomerDto model)
        {
            var customer = await _customerService.GetCustomerById(model.Id);
            if (customer != null)
            {
                await _customerService.DeleteCustomer(customer);
                //activity log
                await _customerActivityService.InsertActivity("DeleteCustomer", customer.Id, _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);
            }
        }

        public virtual async Task<AddressDto> InsertAddress(CustomerDto customer, AddressDto model)
        {
            var address = model.ToEntity();
            address.CreatedOnUtc = DateTime.UtcNow;
            address.Id = "";
            address.CustomerId = customer.Id;
            await _customerService.InsertAddress(address);
            return address.ToModel();
        }
        public virtual async Task<AddressDto> UpdateAddress(CustomerDto customer, AddressDto model)
        {
            var address = (await _customerService.GetCustomerById(customer.Id))?.Addresses.FirstOrDefault(x => x.Id == model.Id);
            address = model.ToEntity(address);
            address.CustomerId = customer.Id;
            await _customerService.UpdateAddress(address);
            return address.ToModel();
        }
        public virtual async Task DeleteAddress(CustomerDto customer, AddressDto model)
        {
            var address = (await _customerService.GetCustomerById(customer.Id))?.Addresses.FirstOrDefault(x => x.Id == model.Id);
            address.CustomerId = customer.Id;
            await _customerService.DeleteAddress(address);
        }

        #region Vendors

        public virtual Task<VendorDto> GetVendorById(string id)
        {
            return _vendor.AsQueryable().FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual IMongoQueryable<VendorDto> GetVendors()
        {
            return _vendor.AsQueryable();
        }

        #endregion
    }
}
