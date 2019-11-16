using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public class CoursesModel : BaseGrandModel
    {
        public CoursesModel()
        {
            CourseList = new List<Course>();
        }

        public List<Course> CourseList { get; set; }
        public string CustomerId { get; set; }

        public class Course : BaseGrandEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
            public string ShortDescription { get; set; }
            public string Level { get; set; }
            public bool Approved { get; set; }
        }
    }

   
}
