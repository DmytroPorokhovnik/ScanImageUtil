using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScanImageUtil.Back
{
    internal class FileStatusLine : INotifyPropertyChanged
    {
        private RenamingStatus status;
        private string engineer = "";
        private string newFileName = "";
        private string serialNumber = "";
        private string bank = "";
        private string date = "";
        private string actNumber = "";
        

        public string NewFileName
        {
            get
            {
                return newFileName;
            }
            set
            {
                newFileName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    Engineer = value.Split('_')[4] ?? "";
                    SerialNumber = value.Split('_')[0] ?? "";
                    Bank = value.Split('_')[3] ?? "";
                    ActNumber = value.Split('_')[2] ?? "";
                    Date = value.Split('_')[1] ?? "";
                }
                OnPropertyChanged("NewFileName");
            }
        }

        public string SourceFilePath { get; set; }

        public string Engineer
        {
            get { return engineer; }
            set
            {
                engineer = value;
                OnPropertyChanged("Engineer");
            }
        }

        public string Bank
        {
            get { return bank; }
            set
            {
                bank = value;
                OnPropertyChanged("Bank");
            }
        }

        public string ActNumber
        {
            get { return actNumber; }
            set
            {
                actNumber = value;
                OnPropertyChanged("ActNumber");
            }
        }

        public string Date
        {
            get { return date; }
            set
            {
                date = value;
                OnPropertyChanged("Date");
            }
        }

        public string SerialNumber
        {
            get { return serialNumber; }
            set
            {
                serialNumber = value;
                OnPropertyChanged("SerialNumber");
            }
        }

        public RenamingStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public FileStatusLine(string newName, string sourceFile, RenamingStatus status)
        {
            NewFileName = newName;
            SourceFilePath = sourceFile;
            Status = status;           
        }

        public FileStatusLine(string newName, string sourceFile)
        {
            NewFileName = newName;
            SourceFilePath = sourceFile;
            Status = RenamingStatus.OK;                 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public enum RenamingStatus
    {
        OK,
        Failed,
        Warned
    }
}
