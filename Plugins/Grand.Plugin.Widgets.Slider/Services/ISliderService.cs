using Grand.Plugin.Widgets.Slider.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.Slider.Services
{
    public partial interface ISliderService
    {
        /// <summary>
        /// Delete a slider
        /// </summary>
        /// <param name="slide">Slide</param>
        Task DeleteSlider(PictureSlider slide);

        /// <summary>
        /// Gets all 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        Task<IList<PictureSlider>> GetPictureSliders();

        /// <summary>
        /// Gets by type 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        Task<IList<PictureSlider>> GetPictureSliders(SliderType sliderType, string objectEntry = "");

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="slideId">Slide identifier</param>
        /// <returns>Tax rate</returns>
        Task<PictureSlider> GetById(string slideId);

        /// <summary>
        /// Inserts a slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        Task InsertPictureSlider(PictureSlider slide);

        /// <summary>
        /// Updates slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        Task UpdatePictureSlider(PictureSlider slide);

        /// <summary>
        /// Delete slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        Task DeletePictureSlider(PictureSlider slide);
    }
}
