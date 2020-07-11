using Grand.Domain.Topics;
using Grand.Web.Areas.Admin.Models.Templates;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class TopicTemplateMappingExtensions
    {
        public static TopicTemplateModel ToModel(this TopicTemplate entity)
        {
            return entity.MapTo<TopicTemplate, TopicTemplateModel>();
        }

        public static TopicTemplate ToEntity(this TopicTemplateModel model)
        {
            return model.MapTo<TopicTemplateModel, TopicTemplate>();
        }

        public static TopicTemplate ToEntity(this TopicTemplateModel model, TopicTemplate destination)
        {
            return model.MapTo(destination);
        }
    }
}