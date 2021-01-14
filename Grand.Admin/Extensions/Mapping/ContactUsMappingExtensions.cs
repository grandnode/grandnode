using Grand.Domain.Messages;
using Grand.Admin.Models.Messages;

namespace Grand.Admin.Extensions
{
    public static class ContactUsMappingExtensions
    {
        public static ContactFormModel ToModel(this ContactUs entity)
        {
            return entity.MapTo<ContactUs, ContactFormModel>();
        }
    }
}