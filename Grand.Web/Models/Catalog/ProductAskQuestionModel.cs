﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductAskQuestionModel : BaseEntityModel
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.FullName")]
        public string FullName { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.Phone")]
        public string Phone { get; set; }

        [GrandResourceDisplayName("Products.AskQuestion.Message")]
        public string Message { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

    }
}