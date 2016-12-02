using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Directory;
using Grand.Services.Events;
using MongoDB.Driver;
using Grand.Core.Domain.Catalog;

namespace Grand.Services.Directory
{
    /// <summary>
    /// Measure dimension service
    /// </summary>
    public partial class MeasureService : IMeasureService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        private const string MEASUREDIMENSIONS_ALL_KEY = "Grand.measuredimension.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : dimension ID
        /// </remarks>
        private const string MEASUREDIMENSIONS_BY_ID_KEY = "Grand.measuredimension.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        private const string MEASUREWEIGHTS_ALL_KEY = "Grand.measureweight.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : weight ID
        /// </remarks>
        private const string MEASUREWEIGHTS_BY_ID_KEY = "Grand.measureweight.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MEASUREDIMENSIONS_PATTERN_KEY = "Grand.measuredimension.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MEASUREWEIGHTS_PATTERN_KEY = "Grand.measureweight.";


        /// <summary>
        /// Key for caching
        /// </summary>
        private const string MEASUREUNITS_ALL_KEY = "Grand.measureunit.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : dimension ID
        /// </remarks>
        private const string MEASUREUNITS_BY_ID_KEY = "Grand.measureunit.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string MEASUREUNITS_PATTERN_KEY = "Grand.measureunit.";

        #endregion

        #region Fields

        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<MeasureUnit> _measureUnitRepository;
        private readonly ICacheManager _cacheManager;
        private readonly MeasureSettings _measureSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="measureDimensionRepository">Dimension repository</param>
        /// <param name="measureWeightRepository">Weight repository</param>
        /// <param name="measureUnitRepository">Unit repository</param>
        /// <param name="measureSettings">Measure settings</param>
        /// <param name="eventPublisher">Event published</param>
        public MeasureService(ICacheManager cacheManager,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<MeasureUnit> measureUnitRepository,
            MeasureSettings measureSettings,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _measureDimensionRepository = measureDimensionRepository;
            _measureWeightRepository = measureWeightRepository;
            _measureUnitRepository = measureUnitRepository;
            _measureSettings = measureSettings;
           _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Dimensions

        /// <summary>
        /// Deletes measure dimension
        /// </summary>
        /// <param name="measureDimension">Measure dimension</param>
        public virtual void DeleteMeasureDimension(MeasureDimension measureDimension)
        {
            if (measureDimension == null)
                throw new ArgumentNullException("measureDimension");

            _measureDimensionRepository.Delete(measureDimension);

            _cacheManager.RemoveByPattern(MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(measureDimension);
        }
        
        /// <summary>
        /// Gets a measure dimension by identifier
        /// </summary>
        /// <param name="measureDimensionId">Measure dimension identifier</param>
        /// <returns>Measure dimension</returns>
        public virtual MeasureDimension GetMeasureDimensionById(string measureDimensionId)
        {
            string key = string.Format(MEASUREDIMENSIONS_BY_ID_KEY, measureDimensionId);
            return _cacheManager.Get(key, () => _measureDimensionRepository.GetById(measureDimensionId));
        }

        /// <summary>
        /// Gets a measure dimension by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <returns>Measure dimension</returns>
        public virtual MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword)
        {
            if (String.IsNullOrEmpty(systemKeyword))
                return null;

            var measureDimensions = GetAllMeasureDimensions();
            foreach (var measureDimension in measureDimensions)
                if (measureDimension.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureDimension;
            return null;
        }

        /// <summary>
        /// Gets all measure dimensions
        /// </summary>
        /// <returns>Measure dimensions</returns>
        public virtual IList<MeasureDimension> GetAllMeasureDimensions()
        {
            string key = MEASUREDIMENSIONS_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from md in _measureDimensionRepository.Table
                            orderby md.DisplayOrder
                            select md;
                var measureDimensions = query.ToList();
                return measureDimensions;

            });
        }

        /// <summary>
        /// Inserts a measure dimension
        /// </summary>
        /// <param name="measure">Measure dimension</param>
        public virtual void InsertMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureDimensionRepository.Insert(measure);

            _cacheManager.RemoveByPattern(MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure dimension
        /// </summary>
        /// <param name="measure">Measure dimension</param>
        public virtual void UpdateMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureDimensionRepository.Update(measure);

            _cacheManager.RemoveByPattern(MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(measure);
        }

        /// <summary>
        /// Converts dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureDimension">Source dimension</param>
        /// <param name="targetMeasureDimension">Target dimension</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertDimension(decimal value, 
            MeasureDimension sourceMeasureDimension, MeasureDimension targetMeasureDimension, bool round = true)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException("sourceMeasureDimension");

            if (targetMeasureDimension == null)
                throw new ArgumentNullException("targetMeasureDimension");

            decimal result = value;
            if (result != decimal.Zero && sourceMeasureDimension.Id != targetMeasureDimension.Id)
            {
                result = ConvertToPrimaryMeasureDimension(result, sourceMeasureDimension);
                result = ConvertFromPrimaryMeasureDimension(result, targetMeasureDimension);
            }
            if (round)
                result = Math.Round(result, 2);
            return result;
        }

        /// <summary>
        /// Converts to primary measure dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureDimension">Source dimension</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertToPrimaryMeasureDimension(decimal value,
            MeasureDimension sourceMeasureDimension)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException("sourceMeasureDimension");

            decimal result = value;
            var baseDimensionIn = GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && sourceMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = sourceMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new GrandException(string.Format("Exchange ratio not set for dimension [{0}]", sourceMeasureDimension.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetMeasureDimension">Target dimension</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertFromPrimaryMeasureDimension(decimal value,
            MeasureDimension targetMeasureDimension)
        {
            if (targetMeasureDimension == null)
                throw new ArgumentNullException("targetMeasureDimension");

            decimal result = value;
            var baseDimensionIn = GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && targetMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = targetMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new GrandException(string.Format("Exchange ratio not set for dimension [{0}]", targetMeasureDimension.Name));
                result = result * exchangeRatio;
            }
            return result;
        }

        #endregion

        #region Weights

        /// <summary>
        /// Deletes measure weight
        /// </summary>
        /// <param name="measureWeight">Measure weight</param>
        public virtual void DeleteMeasureWeight(MeasureWeight measureWeight)
        {
            if (measureWeight == null)
                throw new ArgumentNullException("measureWeight");

            _measureWeightRepository.Delete(measureWeight);

            _cacheManager.RemoveByPattern(MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(measureWeight);
        }

        /// <summary>
        /// Gets a measure weight by identifier
        /// </summary>
        /// <param name="measureWeightId">Measure weight identifier</param>
        /// <returns>Measure weight</returns>
        public virtual MeasureWeight GetMeasureWeightById(string measureWeightId)
        {
            string key = string.Format(MEASUREWEIGHTS_BY_ID_KEY, measureWeightId);
            return _cacheManager.Get(key, () => _measureWeightRepository.GetById(measureWeightId));
        }

        /// <summary>
        /// Gets a measure weight by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <returns>Measure weight</returns>
        public virtual MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword)
        {
            if (String.IsNullOrEmpty(systemKeyword))
                return null;

            var measureWeights = GetAllMeasureWeights();
            foreach (var measureWeight in measureWeights)
                if (measureWeight.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureWeight;
            return null;
        }

        /// <summary>
        /// Gets all measure weights
        /// </summary>
        /// <returns>Measure weights</returns>
        public virtual IList<MeasureWeight> GetAllMeasureWeights()
        {
            string key = MEASUREWEIGHTS_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from mw in _measureWeightRepository.Table
                            orderby mw.DisplayOrder
                            select mw;
                var measureWeights = query.ToList();
                return measureWeights;
            });
        }

        /// <summary>
        /// Inserts a measure weight
        /// </summary>
        /// <param name="measure">Measure weight</param>
        public virtual void InsertMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureWeightRepository.Insert(measure);

            _cacheManager.RemoveByPattern(MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure weight
        /// </summary>
        /// <param name="measure">Measure weight</param>
        public virtual void UpdateMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureWeightRepository.Update(measure);
            
            _cacheManager.RemoveByPattern(MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(measure);
        }

        /// <summary>
        /// Converts weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureWeight">Source weight</param>
        /// <param name="targetMeasureWeight">Target weight</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertWeight(decimal value,
            MeasureWeight sourceMeasureWeight, MeasureWeight targetMeasureWeight, bool round = true)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException("sourceMeasureWeight");

            if (targetMeasureWeight == null)
                throw new ArgumentNullException("targetMeasureWeight");

            decimal result = value;
            if (result != decimal.Zero && sourceMeasureWeight.Id != targetMeasureWeight.Id)
            {
                result = ConvertToPrimaryMeasureWeight(result, sourceMeasureWeight);
                result = ConvertFromPrimaryMeasureWeight(result, targetMeasureWeight);
            }
            if (round)
                result = Math.Round(result, 2);
            return result;
        }

        /// <summary>
        /// Converts to primary measure weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureWeight">Source weight</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertToPrimaryMeasureWeight(decimal value, MeasureWeight sourceMeasureWeight)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException("sourceMeasureWeight");

            decimal result = value;
            var baseWeightIn = GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (result != decimal.Zero && sourceMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = sourceMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new GrandException(string.Format("Exchange ratio not set for weight [{0}]", sourceMeasureWeight.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetMeasureWeight">Target weight</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertFromPrimaryMeasureWeight(decimal value,
            MeasureWeight targetMeasureWeight)
        {
            if (targetMeasureWeight == null)
                throw new ArgumentNullException("targetMeasureWeight");

            decimal result = value;
            var baseWeightIn = GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (result != decimal.Zero && targetMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = targetMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new GrandException(string.Format("Exchange ratio not set for weight [{0}]", targetMeasureWeight.Name));
                result = result * exchangeRatio;
            }
            return result;
        }

        #endregion

        #region MeasureUnit

        /// <summary>
        /// Deletes measure unit
        /// </summary>
        /// <param name="measureUnit">Measure unit</param>
        public virtual void DeleteMeasureUnit(MeasureUnit measureUnit)
        {
            if (measureUnit == null)
                throw new ArgumentNullException("measureUnit");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.UnitId, measureUnit.Id);
            var update = Builders<Product>.Update
                .Set(x => x.UnitId, "");
            var result = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IRepository<Product>>()
                .Collection.UpdateManyAsync(filter, update).Result;

            _measureUnitRepository.Delete(measureUnit);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(measureUnit);
        }

        /// <summary>
        /// Gets a measure unit by identifier
        /// </summary>
        /// <param name="measureUnitId">Measure unit identifier</param>
        /// <returns>Measure dimension</returns>
        public virtual MeasureUnit GetMeasureUnitById(string measureUnitId)
        {
            string key = string.Format(MEASUREUNITS_BY_ID_KEY, measureUnitId);
            return _cacheManager.Get(key, () => _measureUnitRepository.GetById(measureUnitId));
        }

        
        /// <summary>
        /// Gets all measure units
        /// </summary>
        /// <returns>Measure unit</returns>
        public virtual IList<MeasureUnit> GetAllMeasureUnits()
        {
            string key = MEASUREUNITS_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from md in _measureUnitRepository.Table
                            orderby md.DisplayOrder
                            select md;
                var measureUnits = query.ToList();
                return measureUnits;

            });
        }

        /// <summary>
        /// Inserts a measure unit
        /// </summary>
        /// <param name="measure">Measure unit</param>
        public virtual void InsertMeasureUnit(MeasureUnit measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureUnitRepository.Insert(measure);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure unit
        /// </summary>
        /// <param name="measure">Measure unit</param>
        public virtual void UpdateMeasureUnit(MeasureUnit measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            _measureUnitRepository.Update(measure);

            _cacheManager.RemoveByPattern(MEASUREUNITS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(measure);
        }
        #endregion

        #endregion
    }
}