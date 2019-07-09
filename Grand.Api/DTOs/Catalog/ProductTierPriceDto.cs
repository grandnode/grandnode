using FluentValidation.Attributes;
using Grand.Api.Validators.Catalog;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Api.DTOs.Catalog
{
    [Validator(typeof(ProductTierPriceValidator))]
    public partial class ProductTierPriceDto : BaseApiEntityModel
    {
        public string StoreId { get; set; }
        public string CustomerRoleId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
    }
}
