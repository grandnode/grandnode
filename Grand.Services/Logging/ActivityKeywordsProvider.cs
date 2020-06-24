using System.Collections.Generic;

namespace Grand.Services.Logging
{
    public class ActivityKeywordsProvider : IActivityKeywordsProvider
    {
        public virtual IList<string> GetCategorySystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewCategory",
                "EditCategory",
                "PublicStore.ViewCategory",
            };
            return tokens;
        }

        public virtual IList<string> GetCheckoutAttributeSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewCheckoutAttribute",
                "EditCheckoutAttribute",
            };
            return tokens;
        }

        public virtual IList<string> GetContactAttributeSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewContactAttribute",
                "EditContactAttribute",
            };
            return tokens;
        }

        public virtual IList<string> GetCustomerSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewCustomer",
                "EditCustomer",
                "CustomerReminder.AbandonedCart",
                "CustomerReminder.RegisteredCustomer",
                "CustomerReminder.LastActivity",
                "CustomerReminder.LastPurchase",
                "CustomerReminder.Birthday",
                "CustomerReminder.SendCampaign",
                "CustomerAdmin.SendEmail",
                "CustomerAdmin.SendPM",
            };
            return tokens;
        }

        public virtual IList<string> GetCustomerRoleSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewCustomerRole",
                "EditCustomerRole",
            };
            return tokens;
        }

        public virtual IList<string> GetDiscountSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewDiscount",
                "EditDiscount",
            };
            return tokens;
        }

        public virtual IList<string> GetGiftCardSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewGiftCard",
                "EditGiftCard",
            };
            return tokens;
        }

        public virtual IList<string> GetInteractiveFormSystemKeywords()
        {
            var tokens = new List<string> {
                "InteractiveFormEdit",
                "InteractiveFormAdd",
                "PublicStore.InteractiveForm",
            };
            return tokens;
        }

        public virtual IList<string> GetKnowledgebaseArticleSystemKeywords()
        {
            var tokens = new List<string> {
                "CreateKnowledgebaseArticle",
                "UpdateKnowledgebaseArticle",
                "DeleteKnowledgebaseArticle",
            };
            return tokens;
        }

        public virtual IList<string> GetKnowledgebaseCategorySystemKeywords()
        {
            var tokens = new List<string> {
                "UpdateKnowledgebaseCategory",
                "CreateKnowledgebaseCategory",
                "DeleteKnowledgebaseCategory",
            };
            return tokens;
        }

        public virtual IList<string> GetManufacturerSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewManufacturer",
                "EditManufacturer",
                "PublicStore.ViewManufacturer",
            };
            return tokens;
        }

        public virtual IList<string> GetOrderSystemKeywords()
        {
            var tokens = new List<string> {
                "EditOrder",
                "PublicStore.PlaceOrder",
            };
            return tokens;
        }

        public virtual IList<string> GetProductSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewProduct",
                "EditProduct",
                "PublicStore.ViewProduct",
            };
            return tokens;
        }

        public virtual IList<string> GetProductAttributeSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewProductAttribute",
                "EditProductAttribute",
            };
            return tokens;
        }

        public virtual IList<string> GetReturnRequestSystemKeywords()
        {
            var tokens = new List<string> {
                "EditReturnRequest",
            };
            return tokens;
        }

        public virtual IList<string> GetSpecificationAttributeSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewSpecAttribute",
                "EditSpecAttribute",
            };
            return tokens;
        }

        public virtual IList<string> GetTopicSystemKeywords()
        {
            var tokens = new List<string> {
                "AddNewTopic",
                "EditTopic",
            };
            return tokens;
        }


    }
}
