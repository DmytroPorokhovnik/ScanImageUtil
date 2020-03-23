using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
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
        static readonly string[] scopes = { SheetsService.Scope.Spreadsheets};
        static readonly string applicationName = "Google Sheets API ScanImageUtil";
        static readonly string sheetsJson = "sheetsCredentials.json";
        static UserCredential credential;

        private int CountRowData(Spreadsheet sheets)
        {
            var count = 0;
            foreach (var sheet in sheets.Sheets)
                count += sheet.Properties.GridProperties.RowCount.Value;
            return count;
        }

        private SheetsService GetTempService()
        {
            using (var stream =
               new FileStream(Path.Combine(Directory.GetCurrentDirectory(),
                   "Resources", sheetsJson), FileMode.Open, FileAccess.Read))
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
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });             
        }

        public GoogleSheetsDbReader(string spreadsheetId)
        {
            this.spreadsheetId = spreadsheetId;
            using (var stream =
                new FileStream(Path.Combine(Directory.GetCurrentDirectory(),
                    "Resources", sheetsJson), FileMode.Open, FileAccess.Read))
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
            request.IncludeGridData = false;
            var generalSpreadSheetInfo = request.Execute();
            var result = new List<ExcelRowDataModel>();
            var countRowsData = CountRowData(generalSpreadSheetInfo);
            var fileProgressWeight = maxProgress / countRowsData;
            var count = 1;
            foreach (var sheet in generalSpreadSheetInfo.Sheets)
            {               
                var dataFilterRequest = new GetSpreadsheetByDataFilterRequest
                {
                    DataFilters = new List<DataFilter>(),
                    IncludeGridData = true
                };
                var dataFilter = new DataFilter
                {
                    GridRange = new GridRange()
                };             
                dataFilter.GridRange.SheetId = sheet.Properties.SheetId.Value;
                dataFilter.GridRange.StartRowIndex = 0;
                dataFilter.GridRange.EndRowIndex = 2999;
                dataFilter.GridRange.StartColumnIndex = 0;
                dataFilter.GridRange.EndColumnIndex = 29;
                dataFilterRequest.DataFilters.Add(dataFilter);
                GC.Collect();
                var dataRequest = sheetsService.Spreadsheets.GetByDataFilter(dataFilterRequest, spreadsheetId);
                var sheetData = dataRequest.Execute();

                foreach (var row in sheetData.Sheets[0].Data[0].RowData)
                {
                    if (row == null || row.Values == null || row.Values.Count == 0 || row.Values[0] == null ||
                        string.IsNullOrEmpty(row.Values[0].FormattedValue) || row.Values[0].FormattedValue == "Серийный номер")
                    {
                        worker?.ReportProgress((int)(fileProgressWeight * count));
                        count++;
                        continue;
                    }
                    result.Add(new ExcelRowDataModel(row));
                    worker?.ReportProgress((int)(fileProgressWeight * count));
                    count++;
                }
            }
            GC.Collect();
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
