using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private ImageFormat GetImageFormatFromFile(string extension)
        {            
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

        private void UpdateProgress(BackgroundWorker progressWorker, int count)
        {
            var progressForOneFile = 100D / sourceFiles.Count;
            progressWorker.ReportProgress((int) (progressForOneFile * count));
        }


        public ImageTransformer(List<string> sourceFiles /*string folder*/)
        {
            converter = new ImageConverter();
            this.sourceFiles = sourceFiles;
        }

        public byte[] Convert(byte[] imageData, ImageFormat format)
        {        
            //var convertedImagePath = Path.Combine(convertedImagePathWithoutExt + GetExtensionFromFormat(format));
            using (var inStream = new MemoryStream(imageData))
            using (var outStream = new MemoryStream())
            {
                var imageStream = Image.FromStream(inStream);
                imageStream.Save(outStream, format);
                imageData = converter.ConvertTo(outStream.ToArray(), typeof(byte[])) as byte[];
                return imageData;
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

        public byte[] Resize(byte[] imageData, int resizePercent)
        {            
            using (var stream = new MemoryStream(imageData))
            {
                var image = Image.FromStream(stream);

                var newHeight = image.Height * (resizePercent / 100);
                var newWidth = image.Width * ((resizePercent / 100));
                var thumbnail = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

                using (var thumbnailStream = new MemoryStream())
                {
                    return thumbnailStream.ToArray();                  
                }
            }
        }

        public bool Run(BackgroundWorker progressWorker, bool isResizeNeeded, bool isCompressNeeded, string formatString, int resizePercent = 75, long qualityPercent = 50)
        {
            try
            {
                var format = GetImageFormatFromFile(formatString);
                var count = 0;
                Parallel.ForEach(sourceFiles, (currentFile) =>
                {
                    var imageData = File.ReadAllBytes(currentFile);

                    if (isResizeNeeded)
                    {
                        imageData = Resize(imageData, resizePercent);
                    }

                    if (isCompressNeeded)
                    {
                        imageData = Compress(imageData, qualityPercent, format);
                    }

                    if (formatString != Path.GetExtension(currentFile))
                    {
                        imageData = Convert(imageData, format);
                    }

                    Save(imageData, currentFile); // TODO: save to chosed folder; file = is old file path!
                    count = Interlocked.Increment(ref count);
                    UpdateProgress(progressWorker, count);
                });
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }


        public byte[] Compress(byte[] imageData, long qualityPercent,  ImageFormat format)
        {
            var encoder = GetEncoder(format);                 

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
                    //image.Save(outStream, encoder, encoderParameters);
                }

                imageData = converter.ConvertTo(outStream.ToArray(), typeof(byte[])) as byte[];
                return imageData;
            }
        }

        public void Save(byte[] imageByteArray, string path)
        {
            var image = converter.ConvertTo(imageByteArray, typeof(Image)) as Image;
            image.Save(path);
        }
    }
}
