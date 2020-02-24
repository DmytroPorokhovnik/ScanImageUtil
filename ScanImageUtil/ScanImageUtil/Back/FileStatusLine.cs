using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    internal class FileStatusLine
    {
        public string NewFileName { get; set; }
        public string SourceFilePath { get; set; }
        public RenamingStatus Status { get; set; }

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
    }

    public enum RenamingStatus
    {
        OK,
        Failed,
        Warned
    }
}
