namespace Grand.Domain.Courses
{
    public class CourseSubject : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the course ident
        /// </summary>
        public string CourseId { get; set; }

    }
}
