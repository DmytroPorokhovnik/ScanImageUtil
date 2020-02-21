using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

namespace ScanImageUtil.Back
{
    class ImageCharacterRecognizer
    {
        private readonly ImageAnnotatorClient googleClient;
        private readonly ImageTransformer imageTransformer;
        private readonly Dictionary<string, string> sourceFileRenamedFileDictionary;

        public ImageCharacterRecognizer(Dictionary<string, string> sourceFileRenamedFileDictionary)
        {
            var googleCredentials = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (googleCredentials == null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", "CloudVision-f52722450390.json"));
            }
            googleClient = ImageAnnotatorClient.Create();
            imageTransformer = new ImageTransformer(sourceFileRenamedFileDictionary);
            this.sourceFileRenamedFileDictionary = sourceFileRenamedFileDictionary;
        } 

        public string RecognizeFromFile(string imagePath)
        {
            //imagePath = "C:\\Users\\dporokhx\\Downloads\\IMG_0004.pdf"; // test code should be removed
            var image = Image.FromFile(imagePath);
            var response = googleClient.DetectDocumentText(image);
            return response.Text;
        }

        public string RecognizeFromBytes(byte[] data)
        {
            var image = Image.FromBytes(data);
            var response = googleClient.DetectDocumentText(image);
            return response.Text;
        }

        public void DumbMethod(BackgroundWorker worker)
        {
            var count = 5;
            for(var i = 0; i < count; i++)
            {
                if (worker.CancellationPending)
                    return;
                var progress = (i + 1D) / count * 100;
                worker.ReportProgress((int) progress);
                Thread.Sleep(1000);               
            }
        }
    }
}
