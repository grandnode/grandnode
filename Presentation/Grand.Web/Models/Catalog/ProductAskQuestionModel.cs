using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Grand.Web.Models.Catalog
{
    [Validator(typeof(ProductAskQuestionValidator))]
    public partial class ProductAskQuestionModel: BaseNopEntityModel
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.AskQuestion.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.AskQuestion.FullName")]
        public string FullName { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.AskQuestion.Phone")]
        public string Phone { get; set; }

        [AllowHtml]
        [GrandResourceDisplayName("Products.AskQuestion.Message")]
        public string Message { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

    }
}