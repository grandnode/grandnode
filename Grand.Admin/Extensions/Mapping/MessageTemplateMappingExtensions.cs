using Grand.Domain.Messages;
using Grand.Admin.Models.Messages;

namespace Grand.Admin.Extensions
{
    public static class MessageTemplateMappingExtensions
    {
        public static MessageTemplateModel ToModel(this MessageTemplate entity)
        {
            return entity.MapTo<MessageTemplate, MessageTemplateModel>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model)
        {
            return model.MapTo<MessageTemplateModel, MessageTemplate>();
        }

        public static MessageTemplate ToEntity(this MessageTemplateModel model, MessageTemplate destination)
        {
            return model.MapTo(destination);
        }
    }
}