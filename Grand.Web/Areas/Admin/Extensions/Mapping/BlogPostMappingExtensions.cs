using Grand.Core.Domain.Blogs;
using Grand.Services.Helpers;
using Grand.Web.Areas.Admin.Models.Blogs;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class BlogPostMappingExtensions
    {
        public static BlogPostModel ToModel(this BlogPost entity, IDateTimeHelper dateTimeHelper)
        {
            var blogpost = entity.MapTo<BlogPost, BlogPostModel>();
            blogpost.StartDate = entity.StartDateUtc.ConvertToUserTime(dateTimeHelper);
            blogpost.EndDate = entity.EndDateUtc.ConvertToUserTime(dateTimeHelper);
            return blogpost;
        }

        public static BlogPost ToEntity(this BlogPostModel model, IDateTimeHelper dateTimeHelper)
        {
            var blogpost = model.MapTo<BlogPostModel, BlogPost>();
            blogpost.StartDateUtc = model.StartDate.ConvertToUtcTime(dateTimeHelper);
            blogpost.EndDateUtc = model.EndDate.ConvertToUtcTime(dateTimeHelper);
            return blogpost;
        }

        public static BlogPost ToEntity(this BlogPostModel model, BlogPost destination, IDateTimeHelper dateTimeHelper)
        {
            var blogpost = model.MapTo(destination);
            blogpost.StartDateUtc = model.StartDate.ConvertToUtcTime(dateTimeHelper);
            blogpost.EndDateUtc = model.EndDate.ConvertToUtcTime(dateTimeHelper);
            return blogpost;
        }
    }
}