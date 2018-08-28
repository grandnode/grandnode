using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Components;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class KnowledgebaseCategories : BaseViewComponent
    {
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;

        public KnowledgebaseCategories(IKnowledgebaseService knowledgebaseService, KnowledgebaseSettings knowledgebaseSettings)
        {
            this._knowledgebaseService = knowledgebaseService;
            this._knowledgebaseSettings = knowledgebaseSettings;
        }

        public IViewComponentResult Invoke(KnowledgebaseHomePageModel model)
        {
            if (!_knowledgebaseSettings.Enabled)
                return Content("");

            var categories = _knowledgebaseService.GetPublicKnowledgebaseCategories();

            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category.ParentCategoryId))
                {
                    var newNode = new KnowledgebaseCategoryModel
                    {
                        Id = category.Id,
                        Name = category.GetLocalized(y => y.Name),
                        Children = new List<KnowledgebaseCategoryModel>(),
                        IsCurrent = model.CurrentCategoryId == category.Id,
                        SeName = category.GetLocalized(y => y.SeName)
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
                    Name = child.GetLocalized(y => y.Name),
                    Children = new List<KnowledgebaseCategoryModel>(),
                    IsCurrent = currentCategoryId == child.Id,
                    SeName = child.GetLocalized(y => y.SeName),
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
