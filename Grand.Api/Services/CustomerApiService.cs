using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Customers;
using Grand.Data;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;

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

        protected void SaveCustomerAttributes(CustomerDto model, Customer customer)
        {
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumber, model.VatNumber);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.VatNumberStatusId, model.VatNumberStatusId);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, model.Gender);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, model.FirstName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, model.LastName);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, model.DateOfBirth);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, model.Company);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, model.StreetAddress);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress2, model.StreetAddress2);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ZipPostalCode, model.ZipPostalCode);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.City, model.City);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, model.CountryId);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StateProvinceId, model.StateProvinceId);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, model.Phone);
            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Fax, model.Fax);
        }

        protected void SaveCustomerRoles(CustomerDto model, Customer customer)
        {
            var insertRoles = model.CustomerRoles.Except(customer.CustomerRoles.Select(x => x.Id)).ToList();
            foreach (var item in insertRoles)
            {
                var role = _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Add(role);
                    role.CustomerId = customer.Id;
                    _customerService.InsertCustomerRoleInCustomer(role);
                }
            }
            var deleteRoles = customer.CustomerRoles.Select(x => x.Id).Except(model.CustomerRoles).ToList();
            foreach (var item in deleteRoles)
            {
                var role = _customerService.GetCustomerRoleById(item);
                if (role != null)
                {
                    customer.CustomerRoles.Remove(customer.CustomerRoles.FirstOrDefault(x => x.Id == role.Id));
                    role.CustomerId = customer.Id;
                    _customerService.DeleteCustomerRoleInCustomer(role);
                }
            }
        }

        public virtual CustomerDto GetByEmail(string email)
        {
            return _customerService.GetCustomerByEmail(email).ToModel();
        }

        public virtual CustomerDto InsertOrUpdateCustomer(CustomerDto model)
        {
            if (string.IsNullOrEmpty(model.Id))
                model = InsertCustomer(model);
            else
                model = UpdateCustomer(model);

            return model;
        }
        public virtual CustomerDto InsertCustomer(CustomerDto model)
        {
            var customer = model.ToEntity();
            customer.CreatedOnUtc = DateTime.UtcNow;
            customer.LastActivityDateUtc = DateTime.UtcNow;
            if (string.IsNullOrEmpty(customer.Username))
                customer.Username = customer.Email;

            _customerService.InsertCustomer(customer);
            SaveCustomerAttributes(model, customer);
            SaveCustomerRoles(model, customer);

            //activity log
            _customerActivityService.InsertActivity("AddNewCustomer", customer.Id, _localizationService.GetResource("ActivityLog.AddNewCustomer"), customer.Id);
            return customer.ToModel();
        }

        public virtual CustomerDto UpdateCustomer(CustomerDto model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            customer = model.ToEntity(customer);
            _customerService.UpdateCustomer(customer);
            SaveCustomerAttributes(model, customer);
            SaveCustomerRoles(model, customer);

            //activity log
            _customerActivityService.InsertActivity("EditCustomer", customer.Id, _localizationService.GetResource("ActivityLog.EditCustomer"), customer.Id);

            return customer.ToModel();
        }

        public virtual void DeleteCustomer(CustomerDto model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer != null)
            {
                _customerService.DeleteCustomer(customer);
                //activity log
                _customerActivityService.InsertActivity("DeleteCustomer", customer.Id, _localizationService.GetResource("ActivityLog.DeleteCustomer"), customer.Id);
            }
        }

        public virtual AddressDto InsertAddress(CustomerDto customer, AddressDto model)
        {
            var address = model.ToEntity();
            address.CreatedOnUtc = DateTime.UtcNow;
            address.Id = "";
            address.CustomerId = customer.Id;
            _customerService.InsertAddress(address);
            return address.ToModel();
        }
        public virtual AddressDto UpdateAddress(CustomerDto customer, AddressDto model)
        {
            var address = _customerService.GetCustomerById(customer.Id)?.Addresses.FirstOrDefault(x => x.Id == model.Id);
            address = model.ToEntity(address);
            address.CustomerId = customer.Id;
            _customerService.UpdateAddress(address);
            return address.ToModel();
        }
        public virtual void DeleteAddress(CustomerDto customer, AddressDto model)
        {
            var address = _customerService.GetCustomerById(customer.Id)?.Addresses.FirstOrDefault(x => x.Id == model.Id);
            address.CustomerId = customer.Id;
            _customerService.DeleteAddress(address);
        }

        #region Vendors

        public virtual VendorDto GetVendorById(string id)
        {
            return _vendor.AsQueryable().FirstOrDefault(x => x.Id == id);
        }

        public virtual IMongoQueryable<VendorDto> GetVendors()
        {
            return _vendor.AsQueryable();
        }

        #endregion
    }
}
