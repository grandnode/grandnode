using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Drawing;
using System.Drawing.Text;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Settings;

namespace Grand.Admin.Models.Settings
{
    [Validator(typeof(MediaSettingsValidator))]
    public partial class MediaSettingsModel : BaseNopModel
    {
        public MediaSettingsModel() { }

        #region Standard Media Settings
        public string ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase")]
        public bool PicturesStoredIntoDatabase { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.AvatarPictureSize")]
        public int AvatarPictureSize { get; set; }
        public bool AvatarPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSize")]
        public int ProductThumbPictureSize { get; set; }
        public bool ProductThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductDetailsPictureSize")]
        public int ProductDetailsPictureSize { get; set; }
        public bool ProductDetailsPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSizeOnProductDetailsPage")]
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
        public bool ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.AssociatedProductPictureSize")]
        public int AssociatedProductPictureSize { get; set; }
        public bool AssociatedProductPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.CategoryThumbPictureSize")]
        public int CategoryThumbPictureSize { get; set; }
        public bool CategoryThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ManufacturerThumbPictureSize")]
        public int ManufacturerThumbPictureSize { get; set; }
        public bool ManufacturerThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.VendorThumbPictureSize")]
        public int VendorThumbPictureSize { get; set; }
        public bool VendorThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.CartThumbPictureSize")]
        public int CartThumbPictureSize { get; set; }
        public bool CartThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MiniCartThumbPictureSize")]
        public int MiniCartThumbPictureSize { get; set; }
        public bool MiniCartThumbPictureSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MaximumImageSize")]
        public int MaximumImageSize { get; set; }
        public bool MaximumImageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.MultipleThumbDirectories")]
        public bool MultipleThumbDirectories { get; set; }
        public bool MultipleThumbDirectories_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.DefaultImageQuality")]
        public int DefaultImageQuality { get; set; }
        public bool DefaultImageQuality_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Media.UseImageCompress")]
        public bool UseImageCompress { get; set; }
        public bool UseImageCompress_OverrideForStore { get; set; }

        #endregion



        #region Watermark Text
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkText")]
        public string WatermarkText { get; set; }
        public bool WatermarkText_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkStyle ")]
        public FontStyle WatermarkStyle { get; set; }
        public bool WatermarkStyle_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkFontColor")]
        public Color WatermarkFontColor { get; set; }
        public bool WatermarkFontColor_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkFontFamily")]
        public GenericFontFamilies WatermarkFontFamily { get; set; }
        public bool WatermarkFontFamily_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkPositionXPercent")]
        public int WatermarkPositionXPercent { get; set; }
        public bool WatermarkPositionXPercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkPositionYPercent")]
        public int WatermarkPositionYPercent { get; set; }
        public bool WatermarkPositionYPercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkFontSizePercent")]
        public int WatermarkFontSizePercent { get; set; }
        public bool WatermarkFontSizePercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOpacityPercent")]
        public int WatermarkOpacityPercent { get; set; }
        public bool WatermarkOpacityPercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkDropShadow")]
        public bool WatermarkDropShadow { get; set; }
        public bool WatermarkDropShadow_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkVertical")]
        public bool WatermarkVertical { get; set; }
        public bool WatermarkVertical_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkRightToLeft")]
        public bool WatermarkRightToLeft { get; set; }
        public bool WatermarkRightToLeft_OverrideForStore { get; set; }
        #endregion

        #region Watermark Misc Options
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkForPicturesAboveSize")]
        public int WatermarkForPicturesAboveSize { get; set; }
        public bool WatermarkForPicturesAboveSize_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.ApplyWatermarkOnPicturesWithOriginalSize")]
        public bool ApplyWatermarkOnPicturesWithOriginalSize { get; set; }
        public bool ApplyWatermarkOnPicturesWithOriginalSize_OverrideForStore { get; set; }
        #endregion

        #region Watermark Overlay
        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOverlayID")]
        public string WatermarkOverlayID { get; set; }
        public bool WatermarkOverlayID_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOverlayPositionXPercent")]
        public int WatermarkOverlayPositionXPercent { get; set; }
        public bool WatermarkOverlayPositionXPercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOverlayPositionYPercent")]
        public int WatermarkOverlayPositionYPercent { get; set; }
        public bool WatermarkOverlayPositionYPercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOverlaySizePercent")]
        public int WatermarkOverlaySizePercent { get; set; }
        public bool WatermarkOverlaySizePercent_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Media.WatermarkOverlayOpacityPercent")]
        public int WatermarkOverlayOpacityPercent { get; set; }
        public bool WatermarkOverlayOpacityPercent_OverrideForStore { get; set; }
        #endregion
    }
}