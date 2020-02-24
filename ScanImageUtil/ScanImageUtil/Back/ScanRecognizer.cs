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

        private UsefulInfoModel GetUsefulInfoFromFile(string sourceFilePath)
        {
            var serialNumberRectangle = new RectangleF(1756, 751, 670, 128);
            var dateRectangle = new RectangleF(830, 2886, 520, 120);
            var actNumberRectangle = new RectangleF(1570, 390, 550, 150);

            byte[] serialNumberCroppedImage = imageTransformer.CropImage(sourceFilePath, serialNumberRectangle);
            byte[] dateRectangleCroppedImage = imageTransformer.CropImage(sourceFilePath, dateRectangle);
            byte[] actNumberCroppedImage = imageTransformer.CropImage(sourceFilePath, actNumberRectangle);

            string textSerialNumber = RecognizeFromBytes(serialNumberCroppedImage);
            string textDate = RecognizeFromBytes(dateRectangleCroppedImage).Trim();
            string textActNumber = RecognizeFromBytes(actNumberCroppedImage).Trim();

            textSerialNumber = new string(textSerialNumber.Where(Char.IsDigit).ToArray());
            textDate = new string(textDate.Where(Char.IsDigit).ToArray());
            textActNumber = new string(textActNumber.Where(Char.IsDigit).ToArray());
            return new UsefulInfoModel { ActNumber = textActNumber, Date = textDate, SerialNumber = textSerialNumber };
        }

        private string GetFullFileNameFromExcel(string excelPath, UsefulInfoModel usefulInfo)
        {
            using (var excelReader = new ExcelWorker(excelPath))
            {
                var bankAndEngineer = excelReader.GetBankAndEngineer(usefulInfo.SerialNumber);
                return string.Format("{0}_{1}_{2}_{3}_{4}", usefulInfo.SerialNumber, usefulInfo.Date, usefulInfo.ActNumber, bankAndEngineer.Key, bankAndEngineer.Value);
            }

        }   

        private string RecognizeFromBytes(byte[] data)
        {
            var image = Image.FromBytes(data);
            var response = googleClient.DetectText(image, context);
            return response.FirstOrDefault().Description;
        }

        public ScanRecognizer()
        {
            var googleCredentials = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (googleCredentials == null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", "CloudVision-f52722450390.json"));
            }
            context = new ImageContext();
            context.LanguageHints.Add("ru");
            context.LanguageHints.Add("en");
            googleClient = ImageAnnotatorClient.Create();
            imageTransformer = new ImageTransformer();
        }


        public void Run(BackgroundWorker worker, List<FileStatusLine> fileStatusLines, string excelPath)
        {
            var count = 1;
            var fileProgressWeight = 100D / fileStatusLines.Count;
            Parallel.ForEach(fileStatusLines, (fileStatusLine, state) =>
            {
                if (worker.CancellationPending)
                {
                    state.Break();
                }
                var usefulInfo = GetUsefulInfoFromFile(fileStatusLine.SourceFilePath);
                worker.ReportProgress((int)(fileProgressWeight / 2 * count));
                fileStatusLine.NewFileName = GetFullFileNameFromExcel(excelPath, usefulInfo);
                fileStatusLine.Status = Helper.CheckFileNameRequirements(fileStatusLine.NewFileName) ? RenamingStatus.OK : RenamingStatus.Failed;
                worker.ReportProgress((int)(fileProgressWeight * count));
                count = Interlocked.Increment(ref count);
            });
        }
    }
}
