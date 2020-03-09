using Grand.Web.Models.News;
using MediatR;

namespace Grand.Web.Features.Models.News
{
    public class GetHomePageNewsItems : IRequest<HomePageNewsItemsModel>
    {
    }
}
