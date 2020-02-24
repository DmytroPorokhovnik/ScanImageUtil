using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    internal class FileStatusLine : INotifyPropertyChanged
    {
        private RenamingStatus status;
        public string NewFileName { get; set; }
        public string SourceFilePath { get; set; }
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public enum RenamingStatus
    {
        OK,
        Failed,
        Warned
    }
}
