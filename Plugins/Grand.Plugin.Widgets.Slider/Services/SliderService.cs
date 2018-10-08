using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Plugin.Widgets.Slider.Services
{
    public partial class SliderService: ISliderService
    {
        #region Fields

        private readonly IRepository<PictureSlider> _reporistoryPictureSlider;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Key for sliders
        /// </summary>
        /// <remarks>
        /// {0} : Store id
        /// {1} : Slider type
        /// {2} : Object entry / categoryId || manufacturerId
        /// </remarks>
        public const string SLIDERS_MODEL_KEY = "Grand.slider-{0}-{1}-{2}";
        public const string SLIDERS_PATTERN_KEY = "Grand.slider";

        #endregion
        public SliderService(IRepository<PictureSlider> reporistoryPictureSlider,
            IStoreContext storeContext, IStoreMappingService storeMappingService,
            ICacheManager cacheManager)
        {
            this._reporistoryPictureSlider = reporistoryPictureSlider;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._cacheManager = cacheManager;
        }
        /// <summary>
        /// Delete a slider
        /// </summary>
        /// <param name="slider">Slider</param>
        public virtual void DeleteSlider(PictureSlider slide)
        {
            if(slide==null)
                throw new ArgumentNullException("slide");

            //clear cache
            _cacheManager.RemoveByPattern(SLIDERS_PATTERN_KEY);

            _reporistoryPictureSlider.Delete(slide);
        }

        /// <summary>
        /// Gets all 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        public virtual IList<PictureSlider> GetPictureSliders()
        {
            return _reporistoryPictureSlider.Table.OrderBy(x=>x.SliderTypeId).ThenBy(x=>x.DisplayOrder).ToList();
        }

        /// <summary>
        /// Gets by type 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        public virtual IList<PictureSlider> GetPictureSliders(SliderType sliderType, string objectEntry = "")
        {

            string cacheKey = string.Format(SLIDERS_MODEL_KEY, _storeContext.CurrentStore.Id, sliderType.ToString(), objectEntry);
            return _cacheManager.Get(cacheKey, () =>
            {
                var query = from s in _reporistoryPictureSlider.Table
                            where s.SliderTypeId == (int)sliderType && s.Published
                            select s;

                if (!string.IsNullOrEmpty(objectEntry))
                    query = query.Where(x => x.ObjectEntry == objectEntry);

                return query.ToList().Where(c => _storeMappingService.Authorize(c)).ToList();
            });
        }


        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="slideId">Slide identifier</param>
        /// <returns>Tax rate</returns>
        public virtual PictureSlider GetById(string slideId)
        {
            return _reporistoryPictureSlider.Table.FirstOrDefault(x => x.Id == slideId);
        }

        /// <summary>
        /// Inserts a slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual void InsertPictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            _cacheManager.RemoveByPattern(SLIDERS_PATTERN_KEY);

            _reporistoryPictureSlider.Insert(slide);
        }

        /// <summary>
        /// Updates slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual void UpdatePictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            _cacheManager.RemoveByPattern(SLIDERS_PATTERN_KEY);

            _reporistoryPictureSlider.Update(slide);
        }

        /// <summary>
        /// Delete slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual void DeletePictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            _cacheManager.RemoveByPattern(SLIDERS_PATTERN_KEY);

            _reporistoryPictureSlider.Delete(slide);
        }

    }
}
