using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class InteractiveFormMappingExtensions
    {
        public static InteractiveFormModel ToModel(this InteractiveForm entity)
        {
            return entity.MapTo<InteractiveForm, InteractiveFormModel>();
        }

        public static InteractiveForm ToEntity(this InteractiveFormModel model)
        {
            return model.MapTo<InteractiveFormModel, InteractiveForm>();
        }

        public static InteractiveForm ToEntity(this InteractiveFormModel model, InteractiveForm destination)
        {
            return model.MapTo(destination);
        }

        public static InteractiveFormAttributeModel ToModel(this InteractiveForm.FormAttribute entity)
        {
            return entity.MapTo<InteractiveForm.FormAttribute, InteractiveFormAttributeModel>();
        }

        public static InteractiveForm.FormAttribute ToEntity(this InteractiveFormAttributeModel model)
        {
            return model.MapTo<InteractiveFormAttributeModel, InteractiveForm.FormAttribute>();
        }

        public static InteractiveForm.FormAttribute ToEntity(this InteractiveFormAttributeModel model, InteractiveForm.FormAttribute destination)
        {
            return model.MapTo(destination);
        }

        public static InteractiveFormAttributeValueModel ToModel(this InteractiveForm.FormAttributeValue entity)
        {
            return entity.MapTo<InteractiveForm.FormAttributeValue, InteractiveFormAttributeValueModel>();
        }

        public static InteractiveForm.FormAttributeValue ToEntity(this InteractiveFormAttributeValueModel model)
        {
            return model.MapTo<InteractiveFormAttributeValueModel, InteractiveForm.FormAttributeValue>();
        }

        public static InteractiveForm.FormAttributeValue ToEntity(this InteractiveFormAttributeValueModel model, InteractiveForm.FormAttributeValue destination)
        {
            return model.MapTo(destination);
        }
    }
}