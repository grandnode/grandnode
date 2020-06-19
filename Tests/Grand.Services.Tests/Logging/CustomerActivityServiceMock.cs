using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Core.Domain.Logging;
using Grand.Services.Logging;
using Moq;

namespace Grand.Services.Tests.Logging
{
    class CustomerActivityServiceMock : Mock<ICustomerActivityService>
    {
        public CustomerActivityServiceMock()
        {
            Setup(o => o.GetActivityTypeById(It.IsAny<string>()))
                .Returns<string>(a => Task.Run(() => ActivityTypes[a]));
        }

        private static Dictionary<string, ActivityLogType> ActivityTypes =>
            new Dictionary<string, ActivityLogType>
            {
                {"5ea45e9c8258183ea0dbcaf1", new ActivityLogType {SystemKeyword = "AddNewCategory"} },
                {"5ea45e9c8258183ea0dbcaf2", new ActivityLogType {SystemKeyword = "AddNewCheckoutAttribute"} },
                {"5ea45e9c8258183ea0dbcaf3", new ActivityLogType {SystemKeyword = "AddNewContactAttribute"} },
                {"5ea45e9c8258183ea0dbcaf4", new ActivityLogType {SystemKeyword = "AddNewCustomer"} },
                {"5ea45e9c8258183ea0dbcaf5", new ActivityLogType {SystemKeyword = "AddNewCustomerRole"} },
                {"5ea45e9c8258183ea0dbcaf6", new ActivityLogType {SystemKeyword = "AddNewDiscount"} },
                {"5ea45e9c8258183ea0dbcaf9", new ActivityLogType {SystemKeyword = "AddNewGiftCard"} },
                {"5ea45e9c8258183ea0dbcb27", new ActivityLogType {SystemKeyword = "InteractiveFormEdit"} },
                {"5ea45e9c8258183ea0dbcb51", new ActivityLogType {SystemKeyword = "CreateKnowledgebaseArticle"} },
                {"5ea45e9c8258183ea0dbcb4e", new ActivityLogType {SystemKeyword = "UpdateKnowledgebaseCategory"} },
                {"5ea45e9c8258183ea0dbcafa", new ActivityLogType {SystemKeyword = "AddNewManufacturer"} },
                {"5ea45e9c8258183ea0dbcb1e", new ActivityLogType {SystemKeyword = "EditOrder"} },
                {"5ea45e9c8258183ea0dbcafb", new ActivityLogType {SystemKeyword = "AddNewProduct"} },
                {"5ea45e9c8258183ea0dbcafc", new ActivityLogType {SystemKeyword = "AddNewProductAttribute"} },
                {"5ea45e9c8258183ea0dbcb22", new ActivityLogType {SystemKeyword = "EditReturnRequest"} },
                {"5ea45e9c8258183ea0dbcafe", new ActivityLogType {SystemKeyword = "AddNewSpecAttribute"} },
                {"5ea45e9c8258183ea0dbcaff", new ActivityLogType {SystemKeyword = "AddNewTopic"} },
                {"5ea45e9c8258183ea0dbcb4c", new ActivityLogType {SystemKeyword = "SendEmailFromAdminPanel"} },
                {"5ea45e9c8258183ea0dbcb25", new ActivityLogType {SystemKeyword = "EditTopic"} },
                {"5ea45e9c8258183ea0dbcb0d", new ActivityLogType {SystemKeyword = "DeleteProduct"} },
                {"5ea45e9c8258183ea0dbcb28", new ActivityLogType {SystemKeyword = "InteractiveFormAdd"} },
                {"5ea45e9c8258183ea0dbcb23", new ActivityLogType {SystemKeyword = "EditSettings"} },
                {"5ea45e9c8258183ea0dbcb24", new ActivityLogType {SystemKeyword = "EditSpecAttribute"} },
                {"5ea45e9c8258183ea0dbcb4f", new ActivityLogType {SystemKeyword = "CreateKnowledgebaseCategory"} },
                {"5ea45e9c8258183ea0dbcb1f", new ActivityLogType {SystemKeyword = "EditProduct"} },
                {"5ea45e9c8258183ea0dbcb14", new ActivityLogType {SystemKeyword = "EditCategory"} },
                {"5ea45e9c8258183ea0dbcb1d", new ActivityLogType {SystemKeyword = "EditManufacturer"} },
                {"5ea45e9c8258183ea0dbcb20", new ActivityLogType {SystemKeyword = "EditProductAttribute"} }
            };
    }
}
