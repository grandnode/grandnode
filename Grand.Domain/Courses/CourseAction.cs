namespace Grand.Domain.Courses
{
    public class CourseAction : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer ident
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the course ident
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets the lesson ident
        /// </summary>
        public string LessonId { get; set; }

        /// <summary>
        /// Gets or sets the finished
        /// </summary>
        public bool Finished { get; set; }

    }
}
