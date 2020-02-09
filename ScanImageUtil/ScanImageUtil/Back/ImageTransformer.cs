using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    class ImageTransformer
    {
        private readonly ImageConverter converter;
        private readonly List<string> sourceFiles;


        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            throw new ArgumentException(string.Format("No such format {0} can be compressed", format.ToString()));
        }

        private ImageFormat GetImageFormatFromFile(string imageFilePath)
        {
            var extension = Path.GetExtension(imageFilePath);
            switch (extension)
            {
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                case ".tiff":
                    return ImageFormat.Tiff;
            }
            throw new ArgumentException("Unsupported image format");
        }

        private string GetExtensionFromFormat(ImageFormat format)
        {
            if (format.Guid == ImageFormat.Jpeg.Guid)
                return ".jpg";
            else if (format.Guid == ImageFormat.Png.Guid)
                return ".png";
            else if (format.Guid == ImageFormat.Gif.Guid)
                return ".gif";
            else if (format.Guid == ImageFormat.Tiff.Guid)
                return ".tiff";

            throw new ArgumentException("Unsupported image format");
        }


        public ImageTransformer(List<string> sourceFiles /*string folder*/)
        {
            converter = new ImageConverter();
            this.sourceFiles = sourceFiles;
        }

        public void Convert(string imagePath, string convertedImagePathWithoutExt, ImageFormat format)
        {
            var imageData = File.ReadAllBytes(imagePath);
            var convertedImagePath = Path.Combine(convertedImagePathWithoutExt + GetExtensionFromFormat(format));
            using (var inStream = new MemoryStream(imageData))
            using (var outStream = new MemoryStream())
            {
                var imageStream = Image.FromStream(inStream);
                imageStream.Save(outStream, format);
                var convertedImage = converter.ConvertTo(outStream.ToArray(), typeof(Image)) as Image;
                // convertedImage.Save(convertedImagePath);
            }
        }

        public byte[] CropImage(Image img, RectangleF cropArea)
        {
            var bmpImage = new Bitmap(img);
            var imageConverter = new ImageConverter();
            var croppedImage = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            return imageConverter.ConvertTo(croppedImage, typeof(byte[])) as byte[];
        }

        public byte[] CropImage(string imgPath, RectangleF cropArea)
        {
            var bmpImage = new Bitmap(imgPath);
            var imageConverter = new ImageConverter();
            var croppedImage = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            return imageConverter.ConvertTo(croppedImage, typeof(byte[])) as byte[];
        }

        public void Resize(string imagePath, int resizePercent, string resizedImagePathWithoutExt, ImageFormat format = null)
        {
            var imageData = File.ReadAllBytes(imagePath);
            if (format == null)
                format = GetImageFormatFromFile(imagePath);
            var resizedImagePath = Path.Combine(resizedImagePathWithoutExt + GetExtensionFromFormat(format));

            using (var stream = new MemoryStream(imageData))
            {
                var image = Image.FromStream(stream);

                var newHeight = image.Height * (resizePercent / 100);
                var newWidth = image.Width * ((resizePercent / 100));
                var thumbnail = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

                using (var thumbnailStream = new MemoryStream())
                {    //return                
                    thumbnailStream.ToArray();
                    //resizedImage.Save(resizedImagePath);
                }
            }
        }

        //public bool Run(bool isConvertNeeded, bool isResizeNeeded)
        //{
        //    try {
        //        foreach (var file in sourceFiles)
        //            //{
        //            //    if (isConvertNeeded)
        //            //        Convert()
        //            //if (isResizeNeeded)
        //            //        Resize()    
        //            return true;
        //    }
        //    catch (Exception) {
        //        return false;

        //    }
        //}


        public void CompressConvertAndSave(string imagePath, long qualityPercent, string compressedImagePathWithoutExt, ImageFormat format = null)
        {
            var imageData = File.ReadAllBytes(imagePath);
            var encoder = GetEncoder(format);
            if (format == null)
                format = GetImageFormatFromFile(imagePath);
            var compressedImagePath = Path.Combine(compressedImagePathWithoutExt + GetExtensionFromFormat(format));

            using (var inStream = new MemoryStream(imageData))
            using (var outStream = new MemoryStream())
            {
                var image = Image.FromStream(inStream);

                // if we aren't able to retrieve our encoder
                // we should just save the current image and
                // return to prevent any exceptions from happening
                if (encoder == null)
                {
                    image.Save(outStream, format);
                }
                else
                {
                    var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, qualityPercent);
                    image.Save(outStream, encoder, encoderParameters);
                }

                var compressedImage = converter.ConvertTo(outStream.ToArray(), typeof(Image)) as Image;
                compressedImage.Save(compressedImagePath);
            }
        }

        public void Save(byte[] imageByteArray, string path)
        {
            var image = converter.ConvertTo(imageByteArray, typeof(Image)) as Image;
            image.Save(path);
        }
    }
}
