//using Grand.Api.DTOs.Catalog;
//using Grand.Api.Interfaces;
//using Grand.Api.Tests.Helpers;
//using Grand.Core.Data;
//using Grand.Core.Domain.Catalog;
//using Grand.Services.Security;
//using Grand.Services.Tests;
//using Grand.Web.Areas.Api.Controllers.OData;
//using Microsoft.AspNet.OData.Results;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using MongoDB.Driver.Linq;
//using Moq;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Grand.Api.Tests.ControllerTests
//{
//    [TestClass]
//    public class CategoryControllerTest
//    {
//        private string _id1 = "5c349ef4d595601e04da9dfc";
//        private string _id2 = "5c349ef4d595601e04da9dfd";
//        private string _id3 = "5c349ef4d595601e04da9dfe";
//        private CategoryDto modelInsertOrUpdate = new CategoryDto()
//        {
//            Id = "",
//            Name = "sample category 4"
//        };
//        private IRepository<Category> _categoryRepo;

//        private CategoryController _categoryController;
//        private ICategoryApiService _categoryApiService;
//        private IPermissionService _permissionService;
//        private IMongoCollection<CategoryDto> _categoryDto;

//        [TestInitialize()]
//        public void TestInitialize()
//        {
//            Mongodb.IgnoreExtraElements();
//            InitCategoryRepo();
//            var tempCategoryApiService = new Mock<ICategoryApiService>();
//            {
//                tempCategoryApiService.Setup(instance => instance.GetCategories()).Returns(_categoryDto.AsQueryable());
//                tempCategoryApiService.Setup(instance => instance.GetById(_id1)).ReturnsAsync(_categoryDto.AsQueryable().FirstOrDefault(x => x.Id == _id1));
//                tempCategoryApiService.Setup(instance => instance.InsertOrUpdateCategory(modelInsertOrUpdate)).ReturnsAsync(InsertOrUpdate(modelInsertOrUpdate));

//                _categoryApiService = tempCategoryApiService.Object;
//            }
//            var tempPermissionService = new Mock<IPermissionService>();
//            {
//                tempPermissionService.Setup(instance => instance.Authorize(PermissionSystemName.Categories)).ReturnsAsync(true);
//                _permissionService = tempPermissionService.Object;
//            }
//            _categoryController = new CategoryController(_categoryApiService, _permissionService);

//        }

//        private void InitCategoryRepo()
//        {
//            _categoryRepo = new MongoDBRepositoryTest<Category>();
//            _categoryRepo.Collection.DeleteMany(new BsonDocument());
//            _categoryRepo.Insert(new Category()
//            {
//                Id = _id1,
//                Name = "sample category 1"
//            });
//            _categoryRepo.Insert(new Category()
//            {
//                Id = _id2,
//                Name = "sample category 2"
//            });
//            _categoryRepo.Insert(new Category()
//            {
//                Id = _id3,
//                Name = "sample category 3"
//            });
//            _categoryDto = _categoryRepo.Database.GetCollection<CategoryDto>(typeof(Core.Domain.Catalog.Category).Name);
//        }
//        private CategoryDto InsertOrUpdate(CategoryDto model)
//        {
//            return model;
//        }


//        [TestMethod()]
//        public async Task Can_get_category()
//        {
//            IActionResult result = await _categoryController.Get(_id1);

//            // Assert
//            var okObjectResult = result as OkObjectResult;
//            Assert.IsNotNull(okObjectResult);
//            var presentations = okObjectResult.Value as CategoryDto;
//            Assert.IsNotNull(presentations);
//            Assert.AreEqual("sample category 1", presentations.Name);
//        }

//        [TestMethod()]
//        public async Task Can_get_categories()
//        {
//            IActionResult result = await _categoryController.Get();

//            // Assert
//            var okObjectResult = result as OkObjectResult;
//            Assert.IsNotNull(okObjectResult);
//            var presentations = okObjectResult.Value as IMongoQueryable<CategoryDto>;
//            Assert.IsNotNull(presentations);
//            var count = presentations.Count();
//            Assert.AreEqual(3, count);
//        }

//        [TestMethod()]
//        public async Task Can_Delete()
//        {
//            IActionResult result = await _categoryController.Delete(_id1);
//            // Assert
//            var okResult = result as OkResult;
//            Assert.AreEqual(200, okResult.StatusCode);
//        }

//        [TestMethod()]
//        public async Task Can_Post()
//        {
//            var response = await _categoryController.Post(modelInsertOrUpdate);
//            // Assert
//            var createResult = response as CreatedODataResult<CategoryDto>;
//            Assert.IsNotNull(createResult);
//        }
//    }
//}
