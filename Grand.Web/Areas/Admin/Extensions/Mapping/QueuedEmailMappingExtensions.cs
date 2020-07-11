using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class QueuedEmailMappingExtensions
    {
        public static QueuedEmailModel ToModel(this QueuedEmail entity)
        {
            return entity.MapTo<QueuedEmail, QueuedEmailModel>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model)
        {
            return model.MapTo<QueuedEmailModel, QueuedEmail>();
        }

        public static QueuedEmail ToEntity(this QueuedEmailModel model, QueuedEmail destination)
        {
            return model.MapTo(destination);
        }
    }
}