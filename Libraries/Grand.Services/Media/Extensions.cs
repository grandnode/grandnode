﻿using System;
using System.IO;
using System.Linq;
using System.Web;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Media;
using Grand.Services.Catalog;
using Grand.Core.Infrastructure;

namespace Grand.Services.Media
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the download binary array
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <returns>Download binary array</returns>
        public static byte[] GetDownloadBits(this HttpPostedFileBase postedFile)
        {
            Stream fs = postedFile.InputStream;
            int size = postedFile.ContentLength;
            var binary = new byte[size];
            fs.Read(binary, 0, size);
            return binary;
        }

        /// <summary>
        /// Gets the picture binary array
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <returns>Picture binary array</returns>
        public static byte[] GetPictureBits(this HttpPostedFileBase postedFile)
        {
            Stream fs = postedFile.InputStream;
            int size = postedFile.ContentLength;
            var img = new byte[size];
            fs.Read(img, 0, size);
            return img;
        }

        /// <summary>
        /// Get product picture (for shopping cart and order details pages)
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Atributes (in XML format)</param>
        /// <param name="pictureService">Picture service</param>
        /// <param name="productAttributeParser">Product attribute service</param>
        /// <returns>Picture</returns>
        public static Picture GetProductPicture(this Product product, string attributesXml,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser)
        {
            if (product == null)
                throw new ArgumentNullException("product");
            if (pictureService == null)
                throw new ArgumentNullException("pictureService");
            if (productAttributeParser == null)
                throw new ArgumentNullException("productAttributeParser");

            Picture picture = null;

            //first, let's see whether we have some attribute values with custom pictures
            if (!String.IsNullOrEmpty(attributesXml))
            {
                var attributeValues = productAttributeParser.ParseProductAttributeValues(product, attributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    var attributePicture = pictureService.GetPictureById(attributeValue.PictureId);
                    if (attributePicture != null)
                    {
                        picture = attributePicture;
                        break;
                    }
                }
            }
            //now let's load the default product picture
            if (picture == null)
            {
                var pp = product.ProductPictures.FirstOrDefault();
                if (pp != null)
                    picture = pictureService.GetPictureById(pp.PictureId);
            }

            //let's check whether this product has some parent "grouped" product
            if (picture == null && !product.VisibleIndividually && !String.IsNullOrEmpty(product.ParentGroupedProductId))
            {
                var parentProduct = EngineContext.Current.Resolve<IProductService>().GetProductById(product.ParentGroupedProductId);
                if(parentProduct!=null)
                    if(parentProduct.ProductPictures.Any())
                    {
                        picture = pictureService.GetPictureById(parentProduct.ProductPictures.FirstOrDefault().PictureId);

                    }
            }

            return picture;
        }
    }
}
