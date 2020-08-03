using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Seo;
using Grand.Framework.Extensions;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class BlogViewModelService : IBlogViewModelService
    {
        private readonly IBlogService _blogService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly SeoSettings _seoSettings;

        public BlogViewModelService(IBlogService blogService, IDateTimeHelper dateTimeHelper, IStoreService storeService, IUrlRecordService urlRecordService,
            IPictureService pictureService, ICustomerService customerService, ILocalizationService localizationService, IProductService productService, 
            ICategoryService categoryService, IManufacturerService manufacturerService, IVendorService vendorService,
            ILanguageService languageService, IWorkContext workContext, SeoSettings seoSettings)
        {
            _blogService = blogService;
            _dateTimeHelper = dateTimeHelper;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerService = customerService;
            _localizationService = localizationService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _languageService = languageService;
            _workContext = workContext;
            _seoSettings = seoSettings;
        }

        public virtual async Task<(IEnumerable<BlogPostModel> blogPosts, int totalCount)> PrepareBlogPostsModel(int pageIndex, int pageSize)
        {
            var blogPosts = await _blogService.GetAllBlogPosts(_workContext.CurrentCustomer.StaffStoreId, null, null, pageIndex - 1, pageSize, true);
            return (blogPosts.Select(x =>
                {
                    var m = x.ToModel(_dateTimeHelper);
                    m.Body = "";
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Comments = x.CommentCount;
                    return m;
                }), blogPosts.TotalCount);
        }
        public virtual async Task<BlogPostModel> PrepareBlogPostModel()
        {
            var model = new BlogPostModel();
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);
            //default values
            model.AllowComments = true;
            //locales
            return model;
        }

        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel)
        {
            await blogPostmodel.PrepareStoresMappingModel(null, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);
            return blogPostmodel;
        }
        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost)
        {
            //Store
            await blogPostmodel.PrepareStoresMappingModel(blogPost, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);
            return blogPostmodel;
        }

        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPost blogPost)
        {
            var model = blogPost.ToModel(_dateTimeHelper);
            //Store
            await model.PrepareStoresMappingModel(blogPost, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);
            return model;
        }

        public virtual async Task<BlogPost> InsertBlogPostModel(BlogPostModel model)
        {
            var blogPost = model.ToEntity(_dateTimeHelper);
            blogPost.CreatedOnUtc = DateTime.UtcNow;
            await _blogService.InsertBlogPost(blogPost);

            //search engine name
            var seName = await blogPost.ValidateSeName(model.SeName, model.Title, true, _seoSettings, _urlRecordService, _languageService);
            blogPost.SeName = seName;
            blogPost.Locales = await model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            await _blogService.UpdateBlogPost(blogPost);
            await _urlRecordService.SaveSlug(blogPost, seName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }

        public virtual async Task<BlogPost> UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost)
        {
            string prevPictureId = blogPost.PictureId;
            blogPost = model.ToEntity(blogPost, _dateTimeHelper);
            await _blogService.UpdateBlogPost(blogPost);

            //search engine name
            var seName = await blogPost.ValidateSeName(model.SeName, model.Title, true, _seoSettings, _urlRecordService, _languageService);
            blogPost.SeName = seName;
            blogPost.Locales = await model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            await _blogService.UpdateBlogPost(blogPost);
            await _urlRecordService.SaveSlug(blogPost, seName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != blogPost.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }
        public virtual async Task<(IEnumerable<BlogCommentModel> blogComments, int totalCount)> PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize)
        {
            IList<BlogComment> comments;
            var storeId = string.IsNullOrEmpty(_workContext.CurrentCustomer.StaffStoreId) ? "" : _workContext.CurrentCustomer.StaffStoreId;
            if (!string.IsNullOrEmpty(filterByBlogPostId))
            {
                //filter comments by blog
                var blogPost = await _blogService.GetBlogPostById(filterByBlogPostId);
                comments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
            }
            else
            {
                //load all blog comments
                comments = await _blogService.GetAllComments("", storeId);
            }
            var commentsList = new List<BlogCommentModel>();
            foreach (var blogComment in comments.Skip((pageIndex - 1) * pageSize).Take(pageSize))
            {
                var commentModel = new BlogCommentModel {
                    Id = blogComment.Id,
                    BlogPostId = blogComment.BlogPostId,
                    BlogPostTitle = blogComment.BlogPostTitle,
                    CustomerId = blogComment.CustomerId
                };
                var customer = await _customerService.GetCustomerById(blogComment.CustomerId);
                commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                commentModel.Comment = Core.Html.HtmlHelper.FormatText(blogComment.CommentText, false, true, false, false, false, false);
                commentsList.Add(commentModel);
            }
            return (commentsList, comments.Count);
        }

        public virtual async Task<(IEnumerable<BlogProductModel> blogProducts, int totalCount)> PrepareBlogProductsModel(string filterByBlogPostId, int pageIndex, int pageSize)
        {
            var productModels = new List<BlogProductModel>();
            var blogproducts = await _blogService.GetProductsByBlogPostId(filterByBlogPostId);
            foreach (var item in blogproducts.Skip((pageIndex - 1) * pageSize).Take(pageSize))
            {
                productModels.Add(new BlogProductModel() {
                    Id = item.Id,
                    DisplayOrder = item.DisplayOrder,
                    ProductId = item.ProductId,
                    ProductName = (await _productService.GetProductById(item.ProductId))?.Name
                });
            }
            return (productModels, blogproducts.Count);
        }

        public virtual async Task<BlogProductModel.AddProductModel> PrepareBlogModelAddProductModel(string blogPostId)
        {
            var model = new BlogProductModel.AddProductModel();
            model.BlogPostId = blogPostId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            var storeId = _workContext.CurrentCustomer.StaffStoreId;
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList().ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }

        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(BlogProductModel.AddProductModel model, int pageIndex, int pageSize)
        {
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;
            }
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }

        public virtual async Task InsertProductModel(string blogPostId, BlogProductModel.AddProductModel model)
        {
            foreach (var id in model.SelectedProductIds)
            {
                var products = await _blogService.GetProductsByBlogPostId(blogPostId);
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    if (products.FirstOrDefault(x => x.ProductId == id) == null)
                    {
                        await _blogService.InsertBlogProduct(new BlogProduct() {
                            BlogPostId = blogPostId,
                            ProductId = id,
                            DisplayOrder = 0
                        });
                    }
                }
            }
        }

        public virtual async Task UpdateProductModel(BlogProductModel model)
        {
            var bp = await _blogService.GetBlogProductById(model.Id);
            bp.DisplayOrder = model.DisplayOrder;
            await _blogService.UpdateBlogProduct(bp);
        }

        public virtual async Task DeleteProductModel(string id)
        {
            var bp = await _blogService.GetBlogProductById(id);
            await _blogService.DeleteBlogProduct(bp);
        }
    }
}
