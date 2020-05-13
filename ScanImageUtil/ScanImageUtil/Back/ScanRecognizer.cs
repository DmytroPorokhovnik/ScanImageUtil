using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using RectangleF = System.Drawing.RectangleF;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace ScanImageUtil.Back
{
    class ScanRecognizer
    {
        private readonly ImageAnnotatorClient googleClient;
        private readonly ImageTransformer imageTransformer;
        private readonly ImageContext context;
        private RectangleF serialNumberRectangle;
        private RectangleF dateRectangle;
        private RectangleF actNumberRectangle;
        private RectangleF bankRectangle;

        private UsefulInfoModel GetUsefulInfoFromFile(string sourceFilePath)
        {
            GetCropSettings(sourceFilePath);
            var serialNumberCroppedImage = imageTransformer.CropImage(sourceFilePath, serialNumberRectangle);
            var dateRectangleCroppedImage = imageTransformer.CropImage(sourceFilePath, dateRectangle);
            var actNumberCroppedImage = imageTransformer.CropImage(sourceFilePath, actNumberRectangle);
            var bankCroppedImage = imageTransformer.CropImage(sourceFilePath, bankRectangle);

            var textSerialNumber = RecognizeFromBytes(serialNumberCroppedImage)?.Trim() ?? "";
            var textDate = RecognizeFromBytes(dateRectangleCroppedImage)?.Trim() ?? "";
            var textActNumber = RecognizeFromBytes(actNumberCroppedImage)?.Trim() ?? "";
            var bank = RecognizeFromBytesDocument(bankCroppedImage)?.Trim() ?? "";                

            textSerialNumber = new string(textSerialNumber.Where(Char.IsDigit).ToArray());
            textDate = new string(textDate.Where(Char.IsDigit).ToArray());
            textActNumber = new string(textActNumber.Where(Char.IsDigit).ToArray());
            bank = BankNameManipulator.GetBankName(bank);
            return new UsefulInfoModel { ActNumber = textActNumber, Date = textDate, SerialNumber = textSerialNumber, Bank = bank };
        }

        private void GetCropSettings(string sourcePath)
        {
            var dpi = GetImageDPI(sourcePath);
            switch (dpi)
            {
                case 300:
                    serialNumberRectangle = new RectangleF(1756, 751, 670, 128);
                    dateRectangle = new RectangleF(830, 2886, 520, 120);
                    actNumberRectangle = new RectangleF(1570, 390, 550, 150);
                    bankRectangle = new RectangleF(960, 493, 757, 129);
                    break;
                //case 200:
                //    serialNumberRectangle = new RectangleF(1756, 751, 670, 128);
                //    dateRectangle = new RectangleF(830, 2886, 520, 120);
                //    actNumberRectangle = new RectangleF(1570, 390, 550, 150);
                //    bankRectangle = new RectangleF(960, 493, 757, 129);
                //    break;
                case 150:
                    serialNumberRectangle = new RectangleF(895, 369, 248, 60);
                    dateRectangle = new RectangleF(414, 1441, 233, 43);
                    actNumberRectangle = new RectangleF(791, 201, 223, 55);
                    bankRectangle = new RectangleF(476, 255, 381, 46);
                    break;
                default:
                    throw new Exception(string.Format("Image with {0} dpi isn't supported). Supported dpi: 150, 300", dpi));
            }
        }

        private string GetFullFileName(UsefulInfoModel usefulInfo)
        {
            var date = usefulInfo.Date;
            if (Helper.CheckStraightDate(date))
                date = Helper.GetBackwardDate(date);
            return string.Format("{0}_{1}_№{2}_{3}_{4}", usefulInfo.SerialNumber, date, usefulInfo.ActNumber,
               usefulInfo.Bank, "");

        }

        private string RecognizeFromBytes(byte[] data)
        {
            var image = Image.FromBytes(data);
            var response = googleClient.DetectText(image, context);
            if (response.Count == 0)
                return "";
            return response.FirstOrDefault().Description;
        }

        private string RecognizeFromBytesDocument(byte[] data)
        {
            var image = Image.FromBytes(data);
            var response = googleClient.DetectDocumentText(image, context);
            return response == null ? "" : response.Text;           
        }

        private int GetImageDPI(string sourcePath)
        {
            var image = System.Drawing.Image.FromFile(sourcePath);
            return (int) image.HorizontalResolution;
        }

        public ScanRecognizer()
        {
            var googleCredentials = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (googleCredentials == null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", "CloudVision.json"));
            }
            context = new ImageContext();
            context.LanguageHints.Add("ru");
            context.LanguageHints.Add("uk");
            googleClient = ImageAnnotatorClient.Create();
            imageTransformer = new ImageTransformer();
        }


        public void Run(BackgroundWorker worker, List<FileStatusLine> fileStatusLines, int allProgress = 100)
        {
            var count = 1;
            var fileProgressWeight = (double)allProgress / fileStatusLines.Count;
            Parallel.ForEach(fileStatusLines, (fileStatusLine, state) =>
            {
                if (worker.CancellationPending)
                {
                    state.Break();
                }                
                var usefulInfo = GetUsefulInfoFromFile(fileStatusLine.SourceFilePath);
                worker.ReportProgress((int)(fileProgressWeight / 2 * count));
                fileStatusLine.NewFileName = GetFullFileName(usefulInfo);
                fileStatusLine.Status = Helper.CheckFileNameRequirements(fileStatusLine.NewFileName, false) ? 
                RenamingStatus.OK : RenamingStatus.Failed;
                worker.ReportProgress((100 - allProgress) + (int)(fileProgressWeight * count));
                count = Interlocked.Increment(ref count);
            });
        }        
    }
}
