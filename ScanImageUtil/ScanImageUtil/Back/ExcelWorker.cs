using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ScanImageUtil.Back
{
    class ExcelWorker : IDisposable
    {
        readonly Excel.Application xlApp;
        readonly Excel.Worksheet xlWorkSheet;
        const string bankColumn = "C";
        const string engineerColumn = "AA";
        const string serialNumberColumn = "A";

        private string GetCellValue(int row, string column)
        {
            var value = "";
            try
            {
                value = xlWorkSheet.Cells[row, column].Value.ToString();
                return value;
            }
            catch (Exception e)
            {
                if (e.Message == "Cannot perform runtime binding on a null reference")
                {
                    return value;
                }
                else
                    throw e;
            }
        }

        public ExcelWorker(string excelPath)
        {
            xlApp = new Excel.Application();
            xlWorkSheet = xlApp.Workbooks.Open(excelPath).Worksheets[1];
        }

        public KeyValuePair<string, string> GetBankAndEngineer(string serialNumber)
        {
            var bank = "";
            var engineer = "";
            Excel.Range serialNumberColumnValues = xlWorkSheet.Columns[serialNumberColumn];
            Excel.Range serianNumberRange = serialNumberColumnValues.Find(
               What: serialNumber,
               LookIn: Excel.XlFindLookIn.xlValues,
               LookAt: Excel.XlLookAt.xlPart,
               SearchOrder: Excel.XlSearchOrder.xlByRows,
               SearchDirection: Excel.XlSearchDirection.xlNext
               );
            if (serianNumberRange == null || serianNumberRange.Count != 1)
            {
                return new KeyValuePair<string, string>(bank, engineer);
            }

            bank = GetCellValue(serianNumberRange.Row, bankColumn);
            engineer = GetCellValue(serianNumberRange.Row, engineerColumn);
            return new KeyValuePair<string, string>(bank, engineer);
        }

        public void Dispose()
        {
            xlApp.Quit();
        }
    }
}
