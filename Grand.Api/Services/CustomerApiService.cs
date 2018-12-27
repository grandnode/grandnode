using Grand.Api.DTOs.Customers;
using Grand.Api.Extensions;
using Grand.Core.Domain.Customers;
using Grand.Services.Common;
using Grand.Services.Customers;
using System;
using System.Linq;

namespace Grand.Api.Services
{
    public partial class CustomerApiService : ICustomerApiService
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        public CustomerApiService(ICustomerService customerService, IGenericAttributeService genericAttributeService)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
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
                    customer.CustomerRoles.Remove(customer.CustomerRoles.FirstOrDefault(x=>x.Id == role.Id));
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
            _customerService.InsertCustomer(customer);
            SaveCustomerAttributes(model, customer);
            SaveCustomerRoles(model, customer);
            return customer.ToModel();
        }

        public virtual CustomerDto UpdateCustomer(CustomerDto model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            customer = model.ToEntity(customer);
            _customerService.UpdateCustomer(customer);
            SaveCustomerAttributes(model, customer);
            SaveCustomerRoles(model, customer);
            return customer.ToModel();
        }

        public virtual void DeleteCustomer(CustomerDto model)
        {
            var customer = _customerService.GetCustomerById(model.Id);
            if (customer != null)
                _customerService.DeleteCustomer(customer);
        }
    }
}
