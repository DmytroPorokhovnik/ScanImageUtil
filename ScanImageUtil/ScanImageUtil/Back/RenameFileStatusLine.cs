using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    internal class RenameFileStatusLine
    {
        public string NewFileName { get; private set; }
        public string SourceFilePath { get; private set; }
        public RenamingStatus Status { get; private set; }

        public RenameFileStatusLine(string newName, string sourceFile, RenamingStatus status)
        {
            NewFileName = newName;
            SourceFilePath = sourceFile;
            Status = status;
        }
    }

    internal enum RenamingStatus
    {
        OK,
        Failed,
        Warned
    }
}
