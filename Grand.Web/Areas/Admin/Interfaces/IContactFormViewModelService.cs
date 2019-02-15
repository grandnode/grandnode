using Grand.Core.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System;
using System.Collections.Generic;


namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IContactFormViewModelService
    {
        ContactFormListModel PrepareContactFormListModel();
        ContactFormModel PrepareContactFormModel(ContactUs contactUs);
        (IEnumerable<ContactFormModel> contactFormModel, int totalCount) PrepareContactFormListModel(ContactFormListModel model, int pageIndex, int pageSize);
    }
}
