﻿using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Catalog;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Catalog
{
    [Validator(typeof(ProductReviewValidator))]
    public partial class ProductReviewModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public string ProductId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Product")]
        public string ProductName { get; set; }

        public string Ids {
            get
            {
                return Id.ToString() + ":" + ProductId.ToString();
            }
        }

        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Customer")]
        public string CustomerId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Customer")]
        public string CustomerInfo { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Title")]
        public string Title { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.Rating")]
        public int Rating { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProductReviews.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}