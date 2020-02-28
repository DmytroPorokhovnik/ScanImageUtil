using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using ScanImageUtil.Back.Exceptions;
using ScanImageUtil.Back.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    class GoogleSheetsDbReader : IDisposable
    {
        private readonly SheetsService sheetsService;
        private readonly string spreadsheetId;
        private readonly List<string> serialNumbersInDb;
        static readonly string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static readonly string applicationName = "Google Sheets API ScanImageUtil";
        static UserCredential credential;

        private void ReadAllSerialNumbers()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(spreadsheetId, "A1:A");
            var response = request.Execute();
            foreach (var value in response.Values)
                serialNumbersInDb.Add(value[0].ToString());
        }

        public GoogleSheetsDbReader(string spreadsheetId)
        {
            serialNumbersInDb = new List<string>();
            this.spreadsheetId = spreadsheetId;
            using (var stream =
                new FileStream(Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", "sheetsCredentials.json"), FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
            ReadAllSerialNumbers();
        }

        public ExcelRowDataModel ReadRowBySerialNumber(string serialNumber)
        {
            if (!serialNumbersInDb.Contains(serialNumber))
                throw new RecognizedWrongSerialNumberException("No such serial number was presented in google sheets");
            var range = string.Format("A{0}:AD", serialNumbersInDb.FindIndex((sn) => sn == serialNumber) + 1);
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
           return new ExcelRowDataModel(response.Values[0]);
        }

        public void Dispose()
        {
            sheetsService.Dispose();
        }
    }
}
