namespace Grand.Domain.Courses
{
    public class CourseLesson : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short description
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the course ident
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets the subject ident
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// Gets or sets the video file 
        /// </summary>
        public string VideoFile { get; set; }

        /// <summary>
        /// Gets or sets the attachment ident
        /// </summary>
        public string AttachmentId { get; set; }

        /// <summary>
        /// Gets or sets the picture
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

    }
}
