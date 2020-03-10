using ScanImageUtil.Back.Models;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ScanImageUtil.Back
{
    class ExcelScanWriter : IDisposable
    {
        readonly Excel.Application xlApp;
        readonly Excel.Worksheet xlWorkSheet;

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

        public ExcelScanWriter(string excelPath)
        {
            xlApp = new Excel.Application();
            xlWorkSheet = xlApp.Workbooks.Open(excelPath).Worksheets[1];          
        }

        public void AddRowToEnd(ExcelRowDataModel row)
        {
            var lastCell = xlWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            var rowIndexToInsert = lastCell.Row + 1;
            xlWorkSheet.Cells[rowIndexToInsert, "A"].Value2 = row.SerialNumber;
            xlWorkSheet.Cells[rowIndexToInsert, "B"].Value2 = row.GadgetType;
            xlWorkSheet.Cells[rowIndexToInsert, "C"].Value2 = row.Bank;
            xlWorkSheet.Cells[rowIndexToInsert, "D"].Value2 = row.City;
            xlWorkSheet.Cells[rowIndexToInsert, "E"].Value2 = row.Address;
            xlWorkSheet.Cells[rowIndexToInsert, "F"].Value2 = row.ReactionTime;
            xlWorkSheet.Cells[rowIndexToInsert, "G"].Value2 = row.ATMLogicNumber;
            xlWorkSheet.Cells[rowIndexToInsert, "H"].Value2 = row.ApplicatonNumber;
            xlWorkSheet.Cells[rowIndexToInsert, "I"].Value2 = row.DateAndTime;
            xlWorkSheet.Cells[rowIndexToInsert, "J"].Value2 = row.ContactInfo;
            xlWorkSheet.Cells[rowIndexToInsert, "K"].Value2 = row.ServiceType;
            xlWorkSheet.Cells[rowIndexToInsert, "L"].Value2 = row.AdditionalPayment;
            xlWorkSheet.Cells[rowIndexToInsert, "M"].Value2 = row.Description;
            xlWorkSheet.Cells[rowIndexToInsert, "N"].Value2 = row.DesireDate;
            xlWorkSheet.Cells[rowIndexToInsert, "O"].Value2 = row.TSC;
            xlWorkSheet.Cells[rowIndexToInsert, "P"].Value2 = row.AgreedDate;
            xlWorkSheet.Cells[rowIndexToInsert, "Q"].Value2 = row.ActNumber;
            xlWorkSheet.Cells[rowIndexToInsert, "R"].Value2 = row.ActDate;
            xlWorkSheet.Cells[rowIndexToInsert, "S"].Value2 = row.StartDateTime;
            xlWorkSheet.Cells[rowIndexToInsert, "T"].Value2 = row.EndDateTime;
            xlWorkSheet.Cells[rowIndexToInsert, "U"].Value2 = row.JobsType;
            xlWorkSheet.Cells[rowIndexToInsert, "V"].Value2 = row.WorkDescription;
            xlWorkSheet.Cells[rowIndexToInsert, "W"].Value2 = row.TakenSpareParts;
            xlWorkSheet.Cells[rowIndexToInsert, "X"].Value2 = row.InstalledSpareParts;
            xlWorkSheet.Cells[rowIndexToInsert, "Y"].Value2 = row.ReplaceThermalPaper;
            xlWorkSheet.Cells[rowIndexToInsert, "Z"].Value2 = row.Resume;
            xlWorkSheet.Cells[rowIndexToInsert, "AA"].Value2 = row.Engineer;
            xlWorkSheet.Cells[rowIndexToInsert, "AB"].Value2 = row.SpentTime;
            xlWorkSheet.Cells[rowIndexToInsert, "AC"].Value2 = row.Result;
            xlWorkSheet.Cells[rowIndexToInsert, "AD"].Value2 = row.Explanation;
        }

        public void Dispose()
        {
            xlApp.Quit();
        }
    }
}
