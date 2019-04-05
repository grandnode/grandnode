using Grand.Services.Media;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class UpdatePicture
    {
        public static async Task UpdatePictureSeoNames(this IPictureService pictureService, string pictureId, string name)
        {
            if (!string.IsNullOrEmpty(pictureId))
            {
                var picture = await pictureService.GetPictureById(pictureId);
                if (picture != null)
                    await pictureService.SetSeoFilename(picture.Id, pictureService.GetPictureSeName(name));
            }
        }
    }
}
