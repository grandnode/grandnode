﻿using FluentValidation.Attributes;
using Grand.Api.Validators.Customers;
using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Customers
{
    [Validator(typeof(CustomerRoleValidator))]
    public partial class CustomerRoleDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public bool FreeShipping { get; set; }
        public bool TaxExempt { get; set; }
        public bool Active { get; set; }
        public bool IsSystemRole { get; set; }
        public string SystemName { get; set; }
        public bool EnablePasswordLifetime { get; set; }
    }
}
