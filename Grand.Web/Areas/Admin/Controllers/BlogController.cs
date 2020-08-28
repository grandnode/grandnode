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
using System.Threading.Tasks;
using System.Collections.Generic;
using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Seo;

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
        private readonly IWorkContext _workContext;
        private readonly SeoSettings _seoSettings;

        #endregion

        #region Constructors

        public BlogController(
            IBlogService blogService, 
            IBlogViewModelService blogViewModelService, 
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IStoreService storeService, 
            IWorkContext workContext, 
            SeoSettings seoSettings)
        {
            _blogService = blogService;
            _blogViewModelService = blogViewModelService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeService = storeService;
            _workContext = workContext;
            _seoSettings = seoSettings;
        }

        #endregion

        #region Blog posts

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var blogPosts = await _blogViewModelService.PrepareBlogPostsModel(command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = blogPosts.blogPosts,
                Total = blogPosts.totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = await _blogViewModelService.PrepareBlogPostModel();
            //locales
            await AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(BlogPostModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }
                var blogPost = await _blogViewModelService.InsertBlogPostModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            model = await _blogViewModelService.PrepareBlogPostModel(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var blogPost = await _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogPost.LimitedToStores || (blogPost.LimitedToStores && blogPost.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && blogPost.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Permisions"));
                else
                {
                    if (!blogPost.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = await _blogViewModelService.PrepareBlogPostModel(blogPost);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
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

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(BlogPostModel model, bool continueEditing)
        {
            var blogPost = await _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogPost.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = blogPost.Id });
            }

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                blogPost = await _blogViewModelService.UpdateBlogPostModel(model, blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = blogPost.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);

            model = await _blogViewModelService.PrepareBlogPostModel(model, blogPost);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
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

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var blogPost = await _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogPost.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = blogPost.Id });
            }

            if (ModelState.IsValid)
            {
                await _blogService.DeleteBlogPost(blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Categories
        public IActionResult CategoryList() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> CategoryList(DataSourceRequest command)
        {
            var categories = await _blogService.GetAllBlogCategories(_workContext.CurrentCustomer.StaffStoreId);
            var gridModel = new DataSourceResult {
                Data = categories,
                Total = categories.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> CategoryCreate()
        {
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = new BlogCategoryModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CategoryCreate(BlogCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                var blogCategory = model.ToEntity();
                blogCategory.SeName = SeoExtensions.GetSeName(string.IsNullOrEmpty(blogCategory.SeName) ? blogCategory.Name : blogCategory.SeName, _seoSettings);

                await _blogService.InsertBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Added"));
                return continueEditing ? RedirectToAction("CategoryEdit", new { id = blogCategory.Id }) : RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> CategoryEdit(string id)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogCategory.LimitedToStores || (blogCategory.LimitedToStores && blogCategory.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && blogCategory.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Permisions"));
                else
                {
                    if (!blogCategory.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }

            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = blogCategory.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            await model.PrepareStoresMappingModel(blogCategory, _storeService, false);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CategoryEdit(BlogCategoryModel model, bool continueEditing)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(model.Id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogCategory.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("CategoryEdit", new { id = blogCategory.Id });
            }

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                blogCategory = model.ToEntity(blogCategory);
                blogCategory.SeName = SeoExtensions.GetSeName(string.IsNullOrEmpty(blogCategory.SeName) ? blogCategory.Name : blogCategory.SeName, _seoSettings);
                await _blogService.UpdateBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("CategoryEdit", new { id = blogCategory.Id });
                }
                return RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            await model.PrepareStoresMappingModel(blogCategory, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> CategoryDelete(string id)
        {
            var blogcategory = await _blogService.GetBlogCategoryById(id);
            if (blogcategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogcategory.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("CategoryEdit", new { id = blogcategory.Id });
            }

            if (ModelState.IsValid)
            {
                await _blogService.DeleteBlogCategory(blogcategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Deleted"));
                return RedirectToAction("CategoryList");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("CategoryEdit", new { id = blogcategory.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> CategoryPostList(string categoryId)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");           

            var blogposts = new List<BlogCategoryPost>();
            foreach (var item in blogCategory.BlogPosts)
            {
                var post = new BlogCategoryPost();
                post.Id = item.Id;
                post.BlogPostId = post.BlogPostId;
                var _post = await _blogService.GetBlogPostById(item.BlogPostId);
                if (_post != null)
                    post.Name = _post.Title;

                blogposts.Add(post);
            }
            var gridModel = new DataSourceResult {
                Data = blogposts,
                Total = blogCategory.BlogPosts.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> CategoryPostDelete(string categoryId, string id)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogCategory.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return ErrorForKendoGridJson("blogCategory no permission");
            }

            if (ModelState.IsValid)
            {
                var post = blogCategory.BlogPosts.FirstOrDefault(x => x.Id == id);
                if (post != null)
                {
                    blogCategory.BlogPosts.Remove(post);
                    await _blogService.UpdateBlogCategory(blogCategory);
                }
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> BlogPostAddPopup(string categoryId)
        {
            var model = new AddBlogPostCategoryModel();
            //stores
            var storeId = _workContext.CurrentCustomer.StaffStoreId;

            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            model.CategoryId = categoryId;
            return View(model);
        }
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> BlogPostAddPopupList(DataSourceRequest command, AddBlogPostCategoryModel model)
        {
            var gridModel = new DataSourceResult();

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var posts = await _blogService.GetAllBlogPosts(storeId: model.SearchStoreId, blogPostName: model.SearchBlogTitle, pageIndex: command.Page - 1, pageSize: command.PageSize);
            gridModel.Data = posts.Select(x => new { Id = x.Id, Name = x.Title });
            gridModel.Total = posts.TotalCount;

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> BlogPostAddPopup(AddBlogPostCategoryModel model)
        {
            if (model.SelectedBlogPostIds != null)
            {
                var blogCategory = await _blogService.GetBlogCategoryById(model.CategoryId);
                if (blogCategory != null)
                    foreach (string id in model.SelectedBlogPostIds)
                    {
                        var post = _blogService.GetBlogPostById(id);
                        if (post != null)
                        {
                            if (blogCategory.BlogPosts.Where(x => x.BlogPostId == id).Count() == 0)
                            {
                                blogCategory.BlogPosts.Add(new Domain.Blogs.BlogCategoryPost() { BlogPostId = id });
                                await _blogService.UpdateBlogCategory(blogCategory);
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

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> Comments(string filterByBlogPostId, DataSourceRequest command)
        {
            var model = await _blogViewModelService.PrepareBlogPostCommentsModel(filterByBlogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = model.blogComments,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> CommentDelete(string id)
        {
            var comment = await _blogService.GetBlogCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = await _blogService.GetBlogPostById(comment.BlogPostId);
            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!blogPost.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return ErrorForKendoGridJson("blogPost no permission");
            }

            if (ModelState.IsValid)
            {
                await _blogService.DeleteBlogComment(comment);
                //update totals
                var comments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
                blogPost.CommentCount = comments.Count;
                await _blogService.UpdateBlogPost(blogPost);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion

        #region Products

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> Products(string blogPostId, DataSourceRequest command)
        {
            var model = await _blogViewModelService.PrepareBlogProductsModel(blogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = model.blogProducts,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAddPopup(string blogPostId)
        {
            var model = await _blogViewModelService.PrepareBlogModelAddProductModel(blogPostId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, BlogProductModel.AddProductModel model)
        {
            var products = await _blogViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult {
                Data = products.products.ToList(),
                Total = products.totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(string blogPostId, BlogProductModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _blogViewModelService.InsertProductModel(blogPostId, model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> UpdateProduct(BlogProductModel model)
        {
            if (ModelState.IsValid)
            {
                await _blogViewModelService.UpdateProductModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (ModelState.IsValid)
            {
                await _blogViewModelService.DeleteProductModel(id);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion
    }
}
