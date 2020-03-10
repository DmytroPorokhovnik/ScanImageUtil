using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using ScanImageUtil.Back.Exceptions;
using ScanImageUtil.Back.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ScanImageUtil.Back
{
    class GoogleSheetsDbReader : IDisposable
    {
        private readonly SheetsService sheetsService;
        private readonly string spreadsheetId;
        static readonly string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static readonly string applicationName = "Google Sheets API ScanImageUtil";
        static UserCredential credential;

        public GoogleSheetsDbReader(string spreadsheetId)
        {
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
                ApplicationName = applicationName                
            });            
        }

        public IList<ExcelRowDataModel> ReadAllData(BackgroundWorker worker = null, double maxProgress = 20)
        {
            var request = sheetsService.Spreadsheets.Get(spreadsheetId);
            request.IncludeGridData = true;
            var response = request.Execute();
            var result = new List<ExcelRowDataModel>();
            var fileProgressWeight = maxProgress / response.Sheets[0].Data[0].RowData.Count;
            var count = 1;
            foreach (var row in response.Sheets[0].Data[0].RowData)
            {
                if (row.Values[0].FormattedValue == "Серийный номер" || string.IsNullOrEmpty(row.Values[0].FormattedValue))
                {
                    worker?.ReportProgress((int)(fileProgressWeight * count));
                    count++;
                    continue;
                }
                result.Add(new ExcelRowDataModel(row));                
                worker?.ReportProgress((int)(fileProgressWeight * count));
                count++;
            }
            return result;
        }

        public void Check()
        {
            sheetsService.Spreadsheets.Get(spreadsheetId).Execute();
        }

        public void Dispose()
        {
            sheetsService.Dispose();
        }
    }
}
