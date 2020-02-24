using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanImageUtil.Back
{
    internal static class Helper
    {
        internal static bool CheckFileNameRequirements(string fileName)
        {
            //sn_date_act_bank_engi
            var fileNameParts = fileName.Split('_');
            if (fileNameParts.Length != 5)
                return false;

            if (fileNameParts[1].Length != 6)
                return false;

            if (string.IsNullOrEmpty(fileNameParts[3]) || string.IsNullOrEmpty(fileNameParts[4]))
                return false;

            return true;
        }
    }
}
