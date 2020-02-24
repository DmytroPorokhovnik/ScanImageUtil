using System;
using System.Globalization;

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

            if (fileNameParts[1].Length != 6 && fileNameParts[1].Length != 8)
                return false;
            else
            {
              
                if (fileNameParts[1].Length == 6)
                {
                    try
                    {
                        var dateString = fileNameParts[1].Substring(0, 2) + '/' + fileNameParts[1].Substring(2, 2) + '/' + fileNameParts[1].Substring(4, 2);
                        DateTime.ParseExact(dateString, "dd/MM/yy", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        var dateString = fileNameParts[1].Substring(0, 2) + '/' + fileNameParts[1].Substring(2, 2) + '/' + fileNameParts[1].Substring(4, 4);
                        DateTime.ParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            if (string.IsNullOrEmpty(fileNameParts[3]) || string.IsNullOrEmpty(fileNameParts[4]))
                return false;

            return true;
        }
    }
}
