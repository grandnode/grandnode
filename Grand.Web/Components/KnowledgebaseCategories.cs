using Grand.Core;
using Grand.Domain.Knowledgebase;
using Grand.Framework.Components;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class KnowledgebaseCategories : BaseViewComponent
    {
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly IWorkContext _workContext;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;

        public KnowledgebaseCategories(IKnowledgebaseService knowledgebaseService, IWorkContext workContext, KnowledgebaseSettings knowledgebaseSettings)
        {
            _knowledgebaseService = knowledgebaseService;
            _workContext = workContext;
            _knowledgebaseSettings = knowledgebaseSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(KnowledgebaseHomePageModel model)
        {
            if (!_knowledgebaseSettings.Enabled)
                return Content("");

            var categories = await _knowledgebaseService.GetPublicKnowledgebaseCategories();

            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category.ParentCategoryId))
                {
                    var newNode = new KnowledgebaseCategoryModel
                    {
                        Id = category.Id,
                        Name = category.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                        Children = new List<KnowledgebaseCategoryModel>(),
                        IsCurrent = model.CurrentCategoryId == category.Id,
                        SeName = category.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id)
                    };

                    FillChildNodes(newNode, categories, model.CurrentCategoryId);

                    model.Categories.Add(newNode);
                }
            }

            return View(model);
        }

        public void FillChildNodes(KnowledgebaseCategoryModel parentNode, List<KnowledgebaseCategory> nodes, string currentCategoryId)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.Id);
            foreach (var child in children)
            {
                var newNode = new KnowledgebaseCategoryModel
                {
                    Id = child.Id,
                    Name = child.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                    Children = new List<KnowledgebaseCategoryModel>(),
                    IsCurrent = currentCategoryId == child.Id,
                    SeName = child.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id),
                    Parent = parentNode
                };

                if (newNode.IsCurrent)
                {
                    MarkParentsAsCurrent(newNode);
                }

                FillChildNodes(newNode, nodes, currentCategoryId);

                parentNode.Children.Add(newNode);
            }
        }

        public void MarkParentsAsCurrent(KnowledgebaseCategoryModel node)
        {
            if (node.Parent != null)
            {
                node.Parent.IsCurrent = true;
                MarkParentsAsCurrent(node.Parent);
            }
        }
    }
}
