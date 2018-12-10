using Grand.Services.Media;


namespace Grand.Web.Areas.Admin.Extensions
{
    public static class UpdatePicture
    {
        public static void UpdatePictureSeoNames(this IPictureService pictureService, string pictureId, string name)
        {
            if (!string.IsNullOrEmpty(pictureId))
            {
                var picture = pictureService.GetPictureById(pictureId);
                if (picture != null)
                    pictureService.SetSeoFilename(picture.Id, pictureService.GetPictureSeName(name));
            }
        }
    }
}
