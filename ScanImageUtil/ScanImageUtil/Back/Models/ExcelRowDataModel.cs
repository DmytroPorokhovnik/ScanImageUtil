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
    }
}
