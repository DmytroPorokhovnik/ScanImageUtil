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
        private readonly List<FileStatusLine> fileStatusLines;
        private readonly string savingDir;

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

        private ImageFormat GetImageFormatFromExt(string extension)
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

        //private string GetExtensionFromFormat(ImageFormat format)
        //{
        //    if (format.Guid == ImageFormat.Jpeg.Guid)
        //        return ".jpg";
        //    else if (format.Guid == ImageFormat.Png.Guid)
        //        return ".png";
        //    else if (format.Guid == ImageFormat.Gif.Guid)
        //        return ".gif";
        //    else if (format.Guid == ImageFormat.Tiff.Guid)
        //        return ".tiff";

        //    throw new ArgumentException("Unsupported image format");
        //}

        private void UpdateProgress(BackgroundWorker progressWorker, int count)
        {
            var progressForOneFile = 100D / fileStatusLines.Count;
            progressWorker.ReportProgress((int)(progressForOneFile * count));
        }


        public ImageTransformer(List<FileStatusLine> sourceRenameFilePairs, string folder)
        {
            converter = new ImageConverter();
            this.fileStatusLines = sourceRenameFilePairs;
            savingDir = folder;
        }

        public ImageTransformer()
        {
            converter = new ImageConverter();            
        }

        //public byte[] Convert(byte[] imageData, ImageFormat format)
        //{        
        //    //var convertedImagePath = Path.Combine(convertedImagePathWithoutExt + GetExtensionFromFormat(format));
        //    using (var inStream = new MemoryStream(imageData))
        //    using (var outStream = new MemoryStream())
        //    {
        //        var imageStream = Image.FromStream(inStream);
        //        imageStream.Save(outStream, format);
        //        imageData = converter.ConvertTo(outStream.ToArray(), typeof(byte[])) as byte[];
        //        return imageData;
        //    }
        //}

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

                var newHeight = image.Height * (resizePercent / 100D);
                var newWidth = image.Width * ((resizePercent / 100D));
                var thumbnail = image.GetThumbnailImage((int)newWidth, (int)newHeight, null, IntPtr.Zero);
                var res = converter.ConvertTo(thumbnail, typeof(byte[])) as byte[];
                return res;
            }
        }

        public bool Run(BackgroundWorker progressWorker, bool isResizeNeeded, bool isCompressNeeded, string formatString, int resizePercent = 75, long qualityPercent = 50)
        {
            try
            {
                var format = GetImageFormatFromExt(formatString);
                var count = 0;
                Parallel.ForEach(fileStatusLines, (currentStatusLine, state) =>
                {
                    if (progressWorker.CancellationPending)
                        state.Break();

                    if (currentStatusLine.Status != RenamingStatus.OK)
                        return;

                    var imageData = File.ReadAllBytes(currentStatusLine.SourceFilePath);

                    if (isResizeNeeded)
                    {
                        imageData = Resize(imageData, resizePercent);
                    }

                    if (isCompressNeeded)
                    {
                        imageData = Compress(imageData, qualityPercent, format);
                    }

                    var savingPath = Path.Combine(savingDir, Path.GetFileNameWithoutExtension(currentStatusLine.NewFileName));
                    Save(imageData, savingPath, formatString);
                    count = Interlocked.Increment(ref count);
                    UpdateProgress(progressWorker, count);
                });
                return true;
            }

            catch (Exception e)
            {
                throw e;
            }
        }


        public byte[] Compress(byte[] imageData, long qualityPercent, ImageFormat format)
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
                    image.Save(outStream, encoder, encoderParameters);
                }

                imageData = outStream.ToArray();
                return imageData;
            }
        }

        public void Save(byte[] imageByteArray, string path, string formatString)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(savingDir))
                throw new ArgumentException("No saving directory was specified.");
            var image = converter.ConvertFrom(imageByteArray) as Image;
            image.Save(Path.Combine(savingDir, path + formatString), GetImageFormatFromExt(formatString));
        }
    }
}
