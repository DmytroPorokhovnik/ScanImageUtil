using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;

namespace ScanImageUtil.Back.Models
{
    struct ExcelRowDataModel
    {
        public string SerialNumber;
        public string GadgetType;
        public string ActNumber;
        public string Bank;
        public string City;
        public string Address;
        public string ReactionTime;
        public string ATMLogicNumber;
        public string ApplicatonNumber;
        public string DateAndTime;
        public string ContactInfo;
        public string ServiceType;
        public string AdditionalPayment;
        public string Description;
        public string DesireDate;
        public string TSC;
        public string AgreedDate;
        public string StartDateTime;
        public string EndDateTime;
        public string JobsType;
        public string WorkDescription;
        public string TakenSpareParts;
        public string InstalledSpareParts;
        public string ReplaceThermalPaper;
        public string Resume;
        public string Engineer;
        public string Result;
        public string SpentTime;
        public string Explanation;
        public string ActDate;

        public ExcelRowDataModel(IList<object> valuesFromCloud)
        {
            if (valuesFromCloud.Count < 30)
            {
                while (valuesFromCloud.Count != 30)
                    valuesFromCloud.Add("");
            }
            SerialNumber = valuesFromCloud[0].ToString();
            GadgetType = valuesFromCloud[1].ToString();
            Bank = valuesFromCloud[2].ToString();
            City = valuesFromCloud[3].ToString();
            Address = valuesFromCloud[4].ToString();
            ReactionTime = valuesFromCloud[5].ToString();
            ATMLogicNumber = valuesFromCloud[6].ToString();
            ApplicatonNumber = valuesFromCloud[7].ToString();
            DateAndTime = valuesFromCloud[8].ToString();
            ContactInfo = valuesFromCloud[9].ToString();
            ServiceType = valuesFromCloud[10].ToString();
            AdditionalPayment = valuesFromCloud[11].ToString();
            Description = valuesFromCloud[12].ToString();
            DesireDate = valuesFromCloud[13].ToString();
            TSC = valuesFromCloud[14].ToString();
            AgreedDate = valuesFromCloud[15].ToString();
            ActNumber = valuesFromCloud[16].ToString();
            ActDate = valuesFromCloud[17].ToString();
            StartDateTime = valuesFromCloud[18].ToString();
            EndDateTime = valuesFromCloud[19].ToString();
            JobsType = valuesFromCloud[20].ToString();
            WorkDescription = valuesFromCloud[21].ToString();
            TakenSpareParts = valuesFromCloud[22].ToString();
            InstalledSpareParts = valuesFromCloud[23].ToString();
            ReplaceThermalPaper = valuesFromCloud[24].ToString();
            Resume = valuesFromCloud[25].ToString();
            Engineer = valuesFromCloud[26].ToString();
            SpentTime = valuesFromCloud[27].ToString();
            Result = valuesFromCloud[28].ToString();
            Explanation = valuesFromCloud[29].ToString();
        }

        public ExcelRowDataModel(RowData googleSheetRow)
        {            
            if (googleSheetRow.Values.Count < 30)
            {
                while (googleSheetRow.Values.Count != 30)
                    googleSheetRow.Values.Add(new CellData());
            }
            SerialNumber = googleSheetRow.Values[0].FormattedValue ?? "";
            GadgetType = googleSheetRow.Values[1].FormattedValue ?? "";
            Bank = googleSheetRow.Values[2].FormattedValue ?? "";
            City = googleSheetRow.Values[3].FormattedValue?? "";
            Address = googleSheetRow.Values[4].FormattedValue ?? "";
            ReactionTime = googleSheetRow.Values[5].FormattedValue ?? "";
            ATMLogicNumber = googleSheetRow.Values[6].FormattedValue ?? "";
            ApplicatonNumber = googleSheetRow.Values[7].FormattedValue ?? "";
            DateAndTime = googleSheetRow.Values[8].FormattedValue ?? "";
            ContactInfo = googleSheetRow.Values[9].FormattedValue ?? "";
            ServiceType = googleSheetRow.Values[10].FormattedValue ?? "";
            AdditionalPayment = googleSheetRow.Values[11].FormattedValue ?? "";
            Description = googleSheetRow.Values[12].FormattedValue ?? "";
            DesireDate = googleSheetRow.Values[13].FormattedValue ?? "";
            TSC = googleSheetRow.Values[14].FormattedValue ?? "";
            AgreedDate = googleSheetRow.Values[15].FormattedValue ?? "";
            ActNumber = googleSheetRow.Values[16].FormattedValue ?? "";
            ActDate = googleSheetRow.Values[17].FormattedValue ?? "";
            StartDateTime = googleSheetRow.Values[18].FormattedValue ?? "";
            EndDateTime = googleSheetRow.Values[19].FormattedValue ?? "";
            JobsType = googleSheetRow.Values[20].FormattedValue ?? "";
            WorkDescription = googleSheetRow.Values[21].FormattedValue ?? "";
            TakenSpareParts = googleSheetRow.Values[22].FormattedValue ?? "";
            InstalledSpareParts = googleSheetRow.Values[23].FormattedValue ?? "";
            ReplaceThermalPaper = googleSheetRow.Values[24].FormattedValue ?? "";
            Resume = googleSheetRow.Values[25].FormattedValue ?? "";
            Engineer = googleSheetRow.Values[26].FormattedValue ?? "";
            SpentTime = googleSheetRow.Values[27].FormattedValue ?? "";
            Result = googleSheetRow.Values[28].FormattedValue ?? "";
            Explanation = googleSheetRow.Values[29].FormattedValue ?? "";
        }
    }
}
