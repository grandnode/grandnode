using Grand.Domain.Documents;
using Grand.Web.Areas.Admin.Models.Documents;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class DocumentMappingExtensions
    {
        public static DocumentModel ToModel(this Document entity)
        {
            return entity.MapTo<Document, DocumentModel>();
        }

        public static Document ToEntity(this DocumentModel model)
        {
            return model.MapTo<DocumentModel, Document>();
        }

        public static Document ToEntity(this DocumentModel model, Document destination)
        {
            return model.MapTo(destination);
        }

        public static DocumentTypeModel ToModel(this DocumentType entity)
        {
            return entity.MapTo<DocumentType, DocumentTypeModel>();
        }

        public static DocumentType ToEntity(this DocumentTypeModel model)
        {
            return model.MapTo<DocumentTypeModel, DocumentType>();
        }

        public static DocumentType ToEntity(this DocumentTypeModel model, DocumentType destination)
        {
            return model.MapTo(destination);
        }
    }
}