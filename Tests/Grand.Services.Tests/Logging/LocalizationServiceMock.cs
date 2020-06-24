using System.Collections.Generic;
using Grand.Services.Localization;
using Moq;

namespace Grand.Services.Tests.Logging
{
    class LocalizationServiceMock : Mock<ILocalizationService>
    {
        public LocalizationServiceMock()
        {
            Setup(o => o.GetResource(It.IsAny<string>()))
                .Returns<string>(a => Localizations[a.ToLower()]);
        }

        private static Dictionary<string, string> Localizations =>
            new Dictionary<string, string>
            {
                {"activitylog.addnewcategory", "Added a new category ('{0}')"},
                {"activitylog.addnewcheckoutattribute", "Added a new checkout attribute ('{0}')"},
                {"activitylog.addnewcontactattribute", "Added a new contact attribute ('{0}')"},
                {"activitylog.addnewcustomer", "Added a new customer (ID = {0})"},
                {"activitylog.addnewcustomerrole", "Added a new customer role ('{0}')"},
                {"activitylog.addnewdiscount", "Added a new discount ('{0}')"},
                {"activitylog.addnewgiftcard", "Added a new gift card ('{0}')"},
                {"activitylog.addnewmanufacturer", "Added a new manufacturer ('{0}')"},
                {"activitylog.addnewproduct", "Added a new product ('{0}')"},
                {"activitylog.addnewproductattribute", "Added a new product attribute ('{0}')"},
                {"activitylog.addnewspecattribute", "Added a new specification attribute ('{0}')"},
                {"activitylog.addnewtopic", "Added a new topic ('{0}')"},
                {"activitylog.createknowledgebasearticle", "Created knowledgebase article ('{0}')"},
                {"activitylog.createknowledgebasecategory", "Created knowledgebase category ('{0}')"},
                {"activitylog.deleteproduct", "Deleted a product ('{0}')"},
                {"activitylog.editcategory", "Edited a category ('{0}')"},
                {"activitylog.editmanufacturer", "Edited a manufacturer ('{0}')"},
                {"activitylog.editorder", "Edited an order (ID = {0}). See order notes for details"},
                {"activitylog.editproduct", "Edited a product ('{0}')"},
                {"activitylog.editproductattribute", "Edited a product attribute ('{0}')"},
                {"activitylog.editreturnrequest", "Edited a return request (ID = {0})"},
                {"activitylog.editsettings", "Edited settings"},
                {"activitylog.editspecattribute","Edited a specification attribute ('{0}')"},
                {"activitylog.edittopic", "Edited a topic ('{0}')"},
                {"activitylog.interactiveformadd", "Add Interactive form - {0}"},
                {"activitylog.sendemailfromadminpanel", "Email (from admin panel) {0}"},
                {"activitylog.updateknowledgebasecategory", "Updated knowledgebase category ('{0}')"},

            };
    }
}
