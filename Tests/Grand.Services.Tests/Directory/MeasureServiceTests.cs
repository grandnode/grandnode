using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Directory;
using Grand.Core.Tests.Caching;
using Grand.Services.Events;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Directory.Tests
{
    [TestClass()]
    public class MeasureServiceTests {
        private IRepository<MeasureDimension> _measureDimensionRepository;
        private IRepository<MeasureWeight> _measureWeightRepository;
        private IRepository<MeasureUnit> _measureUnitRepository;
        private MeasureSettings _measureSettings;
        private IMediator _eventPublisher;
        private IMeasureService _measureService;
        private IServiceProvider _serviceProvider;

        private MeasureDimension _measureDimensionInches, _measureDimensionFeets, _measureDimensionMeters, _measureDimensionMilimeters;
        private MeasureWeight _measureWeightOunces, _measureWeightPounds, _measureWeightKilograms, _measureWeightGrams;
        private MeasureUnit _measureUnits;

        [TestInitialize()]
        public void TestInitialize() {
           

            _measureDimensionInches = new MeasureDimension {
                Id = "1",
                Name = "inch(es)",
                SystemKeyword = "inches",
                Ratio = 1M,
                DisplayOrder = 1,
            };
            _measureDimensionFeets = new MeasureDimension {
                Id = "2",
                Name = "feet",
                SystemKeyword = "feet",
                Ratio = 0.08333333M,
                DisplayOrder = 2,
            };
            _measureDimensionMeters = new MeasureDimension {
                Id = "3",
                Name = "meter(s)",
                SystemKeyword = "meters",
                Ratio = 0.0254M,
                DisplayOrder = 3,
            };
            _measureDimensionMilimeters = new MeasureDimension {
                Id = "4",
                Name = "millimetre(s)",
                SystemKeyword = "millimetres",
                Ratio = 25.4M,
                DisplayOrder = 4,
            };

            _measureWeightOunces = new MeasureWeight {
                Id = "1",
                Name = "ounce(s)",
                SystemKeyword = "ounce",
                Ratio = 16M,
                DisplayOrder = 1,
            };
            _measureWeightPounds = new MeasureWeight {
                Id = "2",
                Name = "lb(s)",
                SystemKeyword = "lb",
                Ratio = 1M,
                DisplayOrder = 2,
            };
            _measureWeightKilograms = new MeasureWeight {
                Id = "3",
                Name = "kg(s)",
                SystemKeyword = "kg",
                Ratio = 0.45359237M,
                DisplayOrder = 3,
            };
            _measureWeightGrams = new MeasureWeight {
                Id = "4",
                Name = "gram(s)",
                SystemKeyword = "grams",
                Ratio = 453.59237M,
                DisplayOrder = 4,
            };


            var tempMeasureDimensionRepository = new Mock<IRepository<MeasureDimension>>();
            {
                var IMongoCollection = new Mock<IMongoCollection<MeasureDimension>>().Object;
                IMongoCollection.InsertOne(_measureDimensionInches);
                IMongoCollection.InsertOne(_measureDimensionFeets);
                IMongoCollection.InsertOne(_measureDimensionMeters);
                IMongoCollection.InsertOne(_measureDimensionMilimeters);
                tempMeasureDimensionRepository.Setup(x => x.Table).Returns(IMongoCollection.AsQueryable());
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionInches.Id)).ReturnsAsync(_measureDimensionInches);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionFeets.Id)).ReturnsAsync(_measureDimensionFeets);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionMeters.Id)).ReturnsAsync(_measureDimensionMeters);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionMilimeters.Id)).ReturnsAsync(_measureDimensionMilimeters);

                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionInches.Id)).ReturnsAsync(_measureDimensionInches);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionFeets.Id)).ReturnsAsync(_measureDimensionFeets);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionMeters.Id)).ReturnsAsync(_measureDimensionMeters);
                tempMeasureDimensionRepository.Setup(x => x.GetByIdAsync(_measureDimensionMilimeters.Id)).ReturnsAsync(_measureDimensionMilimeters);

                _measureDimensionRepository = tempMeasureDimensionRepository.Object;
            }

            var tempMeasureWeightRepository = new Mock<IRepository<MeasureWeight>>();
            {
                var IMongoCollection = new Mock<IMongoCollection<MeasureWeight>>().Object;
                IMongoCollection.InsertOne(_measureWeightOunces);
                IMongoCollection.InsertOne(_measureWeightPounds);
                IMongoCollection.InsertOne(_measureWeightKilograms);
                IMongoCollection.InsertOne(_measureWeightGrams);
                tempMeasureWeightRepository.Setup(x => x.Table).Returns(IMongoCollection.AsQueryable());
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightOunces.Id)).ReturnsAsync(_measureWeightOunces);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightPounds.Id)).ReturnsAsync(_measureWeightPounds);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightKilograms.Id)).ReturnsAsync(_measureWeightKilograms);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightGrams.Id)).ReturnsAsync(_measureWeightGrams);

                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightOunces.Id)).ReturnsAsync(_measureWeightOunces);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightPounds.Id)).ReturnsAsync(_measureWeightPounds);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightKilograms.Id)).ReturnsAsync(_measureWeightKilograms);
                tempMeasureWeightRepository.Setup(x => x.GetByIdAsync(_measureWeightGrams.Id)).ReturnsAsync(_measureWeightGrams);
                _measureWeightRepository = tempMeasureWeightRepository.Object;
            }

            _measureUnits = new MeasureUnit
            {
                Id = "1",
                Name = "pcs.",
            };
            var tempMeasureUnitRepository = new Mock<IRepository<MeasureUnit>>();
            {
                var IMongoCollection = new Mock<IMongoCollection<MeasureUnit>>().Object;
                IMongoCollection.InsertOne(_measureUnits);
                tempMeasureUnitRepository.Setup(x => x.Table).Returns(IMongoCollection.AsQueryable());
                tempMeasureUnitRepository.Setup(x => x.GetById(_measureUnits.Id)).Returns(_measureUnits);
                _measureUnitRepository = tempMeasureUnitRepository.Object;
            }

            _measureSettings = new MeasureSettings();
            _measureSettings.BaseDimensionId = _measureDimensionInches.Id; //inch(es) because all other dimensions are in relation to  inches
            _measureSettings.BaseWeightId = _measureWeightPounds.Id; //lb(s) because all other weights are in relation to pounds

            var tempEventPublisher = new Mock<IMediator>();
            {
                //tempEventPublisher.Setup(x => x.PublishAsync(It.IsAny<object>()));
                _eventPublisher = tempEventPublisher.Object;
            }
            _serviceProvider = new Mock<IServiceProvider>().Object;
            _measureService = new MeasureService(new TestMemoryCacheManager(new Mock<IMemoryCache>().Object, _eventPublisher), _measureDimensionRepository,
                _measureWeightRepository, _measureUnitRepository, _measureSettings, _eventPublisher);
        }

        [TestMethod()]
        public async Task Can_convert_dimension() {
            //from meter(s) to feet
           Assert.AreEqual(32.81M, await _measureService.ConvertDimension(10M, _measureDimensionMeters, _measureDimensionFeets, true));
           //from inch(es) to meter(s)
           Assert.AreEqual(0.25M, await _measureService.ConvertDimension(10M, _measureDimensionInches, _measureDimensionMeters, true));
           //from meter(s) to meter(s)
           Assert.AreEqual(13.33M, await _measureService.ConvertDimension(13.333M, _measureDimensionMeters, _measureDimensionMeters, true));
           //from meter(s) to millimeter(s)
           Assert.AreEqual(10000M, await _measureService.ConvertDimension(10M, _measureDimensionMeters, _measureDimensionMilimeters, true));
           //from millimeter(s) to meter(s)
           Assert.AreEqual(10M, await _measureService.ConvertDimension(10000M, _measureDimensionMilimeters, _measureDimensionMeters, true));
        }

        [TestMethod()]
        public async Task Can_convert_weight() {
            //from ounce(s) to lb(s)
            Assert.AreEqual(0.69M, await _measureService.ConvertWeight(11M, _measureWeightOunces, _measureWeightPounds, true));
            //from lb(s) to ounce(s)
            Assert.AreEqual(176M, await _measureService.ConvertWeight(11M, _measureWeightPounds, _measureWeightOunces, true));
            //from ounce(s) to  ounce(s)
            Assert.AreEqual(13.33M, await _measureService.ConvertWeight(13.333M, _measureWeightOunces, _measureWeightOunces, true));
            //from kg(s) to ounce(s)
            Assert.AreEqual(388.01M, await _measureService.ConvertWeight(11M, _measureWeightKilograms, _measureWeightOunces, true));
            //from kg(s) to gram(s)
            Assert.AreEqual(10000M, await _measureService.ConvertWeight(10M, _measureWeightKilograms, _measureWeightGrams, true));
        }
    }
}