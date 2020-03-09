using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Course
{
    public class LessonModel : BaseGrandEntityModel
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public bool DownloadFile { get; set; }
        public bool VideoFile { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string CourseSeName { get; set; }
        public string CourseLevel { get; set; }
        public string CourseDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool Approved { get; set; }
    }
}
