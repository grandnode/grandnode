using Microsoft.AspNetCore.StaticFiles;

namespace Grand.Services.Media
{
    public partial class MimeMappingService : IMimeMappingService
    {
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;

        public MimeMappingService(FileExtensionContentTypeProvider contentTypeProvider)
        {
            _contentTypeProvider = contentTypeProvider;
        }

        public string Map(string filename)
        {
            string contentType;
            if(!_contentTypeProvider.TryGetContentType(filename, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
