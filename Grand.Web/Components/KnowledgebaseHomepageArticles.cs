using Grand.Core;
using Grand.Domain.Knowledgebase;
using Grand.Framework.Components;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class KnowledgebaseHomepageArticles : BaseViewComponent
    {
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly IWorkContext _workContext;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;

        public KnowledgebaseHomepageArticles(IKnowledgebaseService knowledgebaseService, IWorkContext workContext, KnowledgebaseSettings knowledgebaseSettings)
        {
            _knowledgebaseService = knowledgebaseService;
            _workContext = workContext;
            _knowledgebaseSettings = knowledgebaseSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(KnowledgebaseHomePageModel model)
        {
            if (!_knowledgebaseSettings.Enabled)
                return Content("");

            var articles = await _knowledgebaseService.GetHomepageKnowledgebaseArticles();

            foreach (var article in articles)
            {
                var a = new KnowledgebaseItemModel
                {
                    Id = article.Id,
                    Name = article.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                    SeName = article.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id),
                    IsArticle = true
                };

                model.Items.Add(a);
            }

            return View(model);
        }
    }
}
