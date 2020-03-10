﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScanImageUtil.Back
{
    internal class FileStatusLine : INotifyPropertyChanged
    {
        private RenamingStatus status;
        private string engineer = "";
        private string newFileName = "";
        private string serialNumber = "";

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
                    Engineer = value.Split('_')[4] ?? "";
                if (!string.IsNullOrEmpty(value))
                    SerialNumber = value.Split('_')[0] ?? "";
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
