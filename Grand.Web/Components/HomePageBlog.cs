using Grand.Domain.Blogs;
using Grand.Framework.Components;
using Grand.Web.Features.Models.Blogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HomePageBlogViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly BlogSettings _blogSettings;
        public HomePageBlogViewComponent(IMediator mediator,
            BlogSettings blogSettings)
        {
            _mediator = mediator;
            _blogSettings = blogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowBlogOnHomePage)
                return Content("");

            var model = await _mediator.Send(new GetHomePageBlog());
            return View(model);
        }
    }
}