using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScanImageUtil.Back
{
    class ImageCharacterRecognizer
    {
        private readonly ImageAnnotatorClient googleClient;


        public ImageCharacterRecognizer()
        {
            var googleCredentials = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (googleCredentials == null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", "CloudVision-f52722450390.json"));
            }
            googleClient = ImageAnnotatorClient.Create();
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
    }
}
