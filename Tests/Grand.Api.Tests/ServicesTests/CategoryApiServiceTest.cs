//using Grand.Api.Interfaces;
//using Grand.Api.Services;
//using Grand.Api.Tests.Helpers;
//using Grand.Core.Data;
//using Grand.Core.Domain.Catalog;
//using Grand.Core.Domain.Seo;
//using Grand.Services.Catalog;
//using Grand.Services.Localization;
//using Grand.Services.Logging;
//using Grand.Services.Media;
//using Grand.Services.Seo;
//using Grand.Services.Tests;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using Moq;
//using System.Threading.Tasks;

//namespace Grand.Api.Tests.ServicesTests
//{
//    [TestClass()]
//    public class CategoryApiServiceTest
//    {
//        public ICategoryApiService _categoryApiService;

//        private IMongoDBContext _mongoDBContext;
//        private ICategoryService _categoryService;
//        private IUrlRecordService _urlRecordService;
//        private IPictureService _pictureService;

//        private ILocalizationService _localizationService;
//        private ICustomerActivityService _customerActivityService;
//        private ILanguageService _languageService;
//        private SeoSettings _seoSettings;
//        //private IMongoCollection<CategoryDto> _category;
//        private IRepository<Category> _categoryRepo;

//        private string _id1 = "5c349ef4d595601e04da9dfc";
//        private string _id2 = "5c349ef4d595601e04da9dfd";
//        private string _id3 = "5c349ef4d595601e04da9dfe";

//        [TestInitialize()]
//        public void TestInitialize()
//        {
//            Mongodb.IgnoreExtraElements();
//            Mapper.Run();
//            var mongo = Mongodb.MongoDbClient();
//            _mongoDBContext = new MongoDBContext(mongo.client, mongo.database);

//            InitCategoryRepo();

//            var categoryService = new Mock<ICategoryService>();
//            _categoryService = categoryService.Object;


//            var urlRecordService = new Mock<IUrlRecordService>();
//            _urlRecordService = urlRecordService.Object;

//            var pictureService = new Mock<IPictureService>();
//            _pictureService = pictureService.Object;

//            var localizationService = new Mock<ILocalizationService>();
//            _localizationService = localizationService.Object;

//            var customerActivityService = new Mock<ICustomerActivityService>();
//            _customerActivityService = customerActivityService.Object;

//            var languageService = new Mock<ILanguageService>();
//            _languageService = languageService.Object;

//            var seoSettings = new Mock<SeoSettings>();
//            _seoSettings = seoSettings.Object;

//            _categoryApiService = new CategoryApiService(_mongoDBContext, _categoryService, _urlRecordService, _languageService, _pictureService, _customerActivityService, _localizationService, _seoSettings);

//        }
//        private void InitCategoryRepo()
//        {
//            _categoryRepo = new MongoDBRepositoryTest<Category>();
//            _categoryRepo.Collection.DeleteMany(new BsonDocument());
//            _categoryRepo.Insert(new Category() {
//                Id = _id1,
//                Name = "sample category 1"
//            });
//            _categoryRepo.Insert(new Category() {
//                Id = _id2,
//                Name = "sample category 2"
//            });
//            _categoryRepo.Insert(new Category() {
//                Id = _id3,
//                Name = "sample category 3"
//            });
//        }

//        [TestMethod()]
//        public async Task Can_GetById()
//        {
//            var category = await _categoryApiService.GetById(_id1);
//            // Assert
//            Assert.IsNotNull(category);
//            Assert.AreEqual("sample category 1", category.Name);
//        }

//        [TestMethod()]
//        public void Can_GetCategories()
//        {
//            var categories = _categoryApiService.GetCategories();
//            // Assert
//            Assert.AreEqual(3, categories.ToList().Count);
//        }


//        //[TestMethod()]
//        //public void Can_InsertCategory()
//        //{
//        //    var category = new CategoryDto();
//        //    category.Id = "";
//        //    category.Name = "sample category 4";
//        //    //is not possible used insert category method because is used static method - method ValidateSeName, prepared similar method to test
//        //    var insertedcategory = _categoryApiService.InsertCategory(category);
//        //    // Assert
//        //    Assert.IsNotNull(insertedcategory);
//        //    Assert.AreEqual("sample category 4", insertedcategory.Name);
//        //}

//        //[TestMethod()]
//        //public void Can_UpdateCategory()
//        //{
//        //    var category = _categoryApiService.GetById(_id1);
//        //    category.Name = "Update - " + category.Name;
//        //    var updatedcategory = _categoryApiService.UpdateCategory(category);
//        //    // Assert
//        //    Assert.IsNotNull(updatedcategory);
//        //    Assert.AreEqual(category.Name, updatedcategory.Name);
//        //}

//        //[TestMethod()]
//        //public void Can_DeleteCategory()
//        //{
//        //    var category = _categoryApiService.GetById(_id1);
//        //    _categoryApiService.DeleteCategory(category);

//        //    var deletedcategory = _categoryApiService.GetById(_id1);
//        //    // Assert
//        //    Assert.IsNull(deletedcategory);
//        //}

//    }
//}
