using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Blog)]
    public partial class BlogController : BaseAdminController
    {
        #region Fields

        private readonly IBlogService _blogService;
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService, IBlogViewModelService blogViewModelService, ILanguageService languageService, ILocalizationService localizationService,
            IStoreService storeService)
        {
            this._blogService = blogService;
            this._blogViewModelService = blogViewModelService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
        }

        #endregion

        #region Blog posts

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var blogPosts = _blogViewModelService.PrepareBlogPostsModel(command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = blogPosts.blogPosts,
                Total = blogPosts.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = _blogViewModelService.PrepareBlogPostModel();
            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(BlogPostModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var blogPost = _blogViewModelService.InsertBlogPostModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            model = _blogViewModelService.PrepareBlogPostModel(model);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = _blogViewModelService.PrepareBlogPostModel(blogPost);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(BlogPostModel model, bool continueEditing)
        {
            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                blogPost = _blogViewModelService.UpdateBlogPostModel(model, blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = blogPost.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            model = _blogViewModelService.PrepareBlogPostModel(model, blogPost);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _blogService.DeleteBlogPost(blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Categories
        public IActionResult CategoryList() => View();

        [HttpPost]
        public IActionResult CategoryList(DataSourceRequest command)
        {
            var categories = _blogService.GetAllBlogCategories();
            var gridModel = new DataSourceResult
            {
                Data = categories,
                Total = categories.Count
            };
            return Json(gridModel);
        }

        public IActionResult CategoryCreate()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new BlogCategoryModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CategoryCreate(BlogCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var blogCategory = model.ToEntity();
                _blogService.InsertBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Added"));
                return continueEditing ? RedirectToAction("CategoryEdit", new { id = blogCategory.Id }) : RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);

            return View(model);
        }

        public IActionResult CategoryEdit(string id)
        {
            var blogCategory = _blogService.GetBlogCategoryById(id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = blogCategory.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            model.PrepareStoresMappingModel(blogCategory, false, _storeService);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CategoryEdit(BlogCategoryModel model, bool continueEditing)
        {
            var blogCategory = _blogService.GetBlogCategoryById(model.Id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (ModelState.IsValid)
            {
                blogCategory = model.ToEntity(blogCategory);
                _blogService.UpdateBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("CategoryEdit", new { id = blogCategory.Id });
                }
                return RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            model.PrepareStoresMappingModel(blogCategory, true, _storeService);

            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryDelete(string id)
        {
            var blogcategory = _blogService.GetBlogCategoryById(id);
            if (blogcategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (ModelState.IsValid)
            {
                _blogService.DeleteBlogCategory(blogcategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Deleted"));
                return RedirectToAction("CategoryList");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("CategoryEdit", new { id = id });
        }

        [HttpPost]
        public IActionResult CategoryPostList(string categoryId)
        {
            var blogCategory = _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");

            var gridModel = new DataSourceResult
            {
                Data =  blogCategory.BlogPosts.Select(x => new { Id = x.Id, BlogPostId = x.BlogPostId, Name = _blogService.GetBlogPostById(x.BlogPostId)?.Title }),
                Total = blogCategory.BlogPosts.Count
            };
            return Json(gridModel);
        }

        public IActionResult CategoryPostDelete(string categoryId, string id)
        {
            var blogCategory = _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");
            if (ModelState.IsValid)
            {
                var post = blogCategory.BlogPosts.FirstOrDefault(x => x.Id == id);
                if (post != null)
                {
                    blogCategory.BlogPosts.Remove(post);
                    _blogService.UpdateBlogCategory(blogCategory);
                }
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult BlogPostAddPopup(string categoryId)
        {
            var model = new AddBlogPostCategoryModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.CategoryId = categoryId;
            return View(model);
        }

        [HttpPost]
        public IActionResult BlogPostAddPopupList(DataSourceRequest command, AddBlogPostCategoryModel model)
        {
            var gridModel = new DataSourceResult();
            var posts = _blogService.GetAllBlogPosts(storeId: model.SearchStoreId, blogPostName: model.SearchBlogTitle, pageIndex: command.Page - 1, pageSize: command.PageSize);
            gridModel.Data = posts.Select(x => new { Id = x.Id, Name = x.Title });
            gridModel.Total = posts.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult BlogPostAddPopup(AddBlogPostCategoryModel model)
        {
            if (model.SelectedBlogPostIds != null)
            {
                var blogCategory = _blogService.GetBlogCategoryById(model.CategoryId);
                if (blogCategory != null)
                    foreach (string id in model.SelectedBlogPostIds)
                    {
                        var post = _blogService.GetBlogPostById(id);
                        if (post != null)
                        {
                            if (blogCategory.BlogPosts.Where(x => x.BlogPostId == id).Count() == 0)
                            {
                                blogCategory.BlogPosts.Add(new Core.Domain.Blogs.BlogCategoryPost() { BlogPostId = id });
                                _blogService.UpdateBlogCategory(blogCategory);
                            }
                        }
                    }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Comments

        public IActionResult Comments(string filterByBlogPostId)
        {
            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            return View();
        }

        [HttpPost]
        public IActionResult Comments(string filterByBlogPostId, DataSourceRequest command)
        {
            var model = _blogViewModelService.PrepareBlogPostCommentsModel(filterByBlogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = model.blogComments,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        public IActionResult CommentDelete(string id)
        {
            var comment = _blogService.GetBlogCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = _blogService.GetBlogPostById(comment.BlogPostId);
            if (ModelState.IsValid)
            {
                _blogService.DeleteBlogComment(comment);
                //update totals
                blogPost.CommentCount = _blogService.GetBlogCommentsByBlogPostId(blogPost.Id).Count;
                _blogService.UpdateBlogPost(blogPost);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}
