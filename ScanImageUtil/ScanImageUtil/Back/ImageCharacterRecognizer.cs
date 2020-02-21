using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using RectangleF = System.Drawing.RectangleF;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScanImageUtil.Back
{
    class ImageCharacterRecognizer
    {
        private readonly ImageAnnotatorClient googleClient;
        private readonly ImageTransformer imageTransformer;
        private readonly ImageContext context;

        private string GetFileStatusLine(string sourceFilePath)
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

            return string.Format("{0}_{1}_{2}", textSerialNumber, textDate, textActNumber);
        }

        private bool CheckOcr(string fileName)
        {
            //sn_date_act_bank_engi
            var fileNameParts = fileName.Split('_');
            if (fileNameParts[1].Length != 6)
                return false;
            return true;
        }

        private string RecognizeFromFile(string imagePath)
        {
            //imagePath = "C:\\Users\\dporokhx\\Downloads\\IMG_0004.pdf"; // test code should be removed
            var image = Image.FromFile(imagePath);
            var response = googleClient.DetectText(image);
            return response.ToString();
        }

        private string RecognizeFromBytes(byte[] data)
        {
            var image = Image.FromBytes(data);
            var response = googleClient.DetectText(image, context);
            return response.FirstOrDefault().Description;
        }

        public ImageCharacterRecognizer()
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


        public void Run(BackgroundWorker worker, List<FileStatusLine> fileStatusLines)
        {
            var count = 0;
            Parallel.ForEach(fileStatusLines, (fileStatusLine, state) =>
            {
                if (worker.CancellationPending)
                {
                    state.Break();
                }
                fileStatusLine.NewFileName = GetFileStatusLine(fileStatusLine.SourceFilePath);
                fileStatusLine.Status = CheckOcr(fileStatusLine.NewFileName) ? RenamingStatus.OK : RenamingStatus.Failed;
                var progress = (++count) / (double)fileStatusLines.Count * 100;
                worker.ReportProgress((int)progress);
            });
        }
    }
}
