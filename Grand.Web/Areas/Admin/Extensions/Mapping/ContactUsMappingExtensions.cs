using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ContactUsMappingExtensions
    {
        public static ContactFormModel ToModel(this ContactUs entity)
        {
            return entity.MapTo<ContactUs, ContactFormModel>();
        }
    }
}