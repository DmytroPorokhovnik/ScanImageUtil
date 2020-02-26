using System;
using System.Globalization;

namespace ScanImageUtil.Back
{
    internal static class Helper
    {
        internal static bool CheckStraightDate(string dateStr)
        {
            if (dateStr.Length == 6)
            {
                try
                {
                    var res = dateStr.Substring(0, 2) + '/' + dateStr.Substring(2, 2) + '/' + dateStr.Substring(4, 2);
                    DateTime.ParseExact(res, "dd/MM/yy", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else if (dateStr.Length == 8)
            {
                try
                {
                    var res = dateStr.Substring(0, 2) + '/' + dateStr.Substring(2, 2) + '/' + dateStr.Substring(4, 4);
                    DateTime.ParseExact(res, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        internal static bool CheckBackwardDate(string dateStr)
        {
            if (dateStr.Length == 6)
            {
                try
                {
                    var res = dateStr.Substring(0, 2) + '/' + dateStr.Substring(2, 2) + '/' + dateStr.Substring(4, 2);
                    DateTime.ParseExact(res, "yy/MM/dd", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            else if (dateStr.Length == 8)
            {
                try
                {
                    var res = dateStr.Substring(0, 4) + '/' + dateStr.Substring(4, 2) + '/' + dateStr.Substring(6, 2);
                    DateTime.ParseExact(res, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        internal static bool CheckFileNameRequirements(string fileName, bool straightDateFormat = true)
        {
            //sn_date_act_bank_engi
            fileName = fileName.Replace(".", "");
            var fileNameParts = fileName.Split('_');
            if (fileNameParts.Length != 5)
                return false;
            if (!fileNameParts[2].Contains("№"))
                return false;
            if (string.IsNullOrEmpty(fileNameParts[3]) || string.IsNullOrEmpty(fileNameParts[4]))
                return false;

            if (fileNameParts[1].Length != 6 && fileNameParts[1].Length != 8)
                return false;
            else
            {
                var checkDate = straightDateFormat ? CheckStraightDate(fileNameParts[1]) : CheckBackwardDate(fileNameParts[1]);
                if (!checkDate)
                    return false;
            }

            return true;
        }

        internal static string GetBackwardDate(string dateStr)
        {
            if (dateStr.Length == 6)
            {
                return dateStr.Substring(4, 2) + "." + dateStr.Substring(2, 2) + "." + dateStr.Substring(0, 2);
            }
            else if (dateStr.Length == 8)
            {
                return dateStr.Substring(4, 4) + "." + dateStr.Substring(2, 2) + "." + dateStr.Substring(0, 2);
            }
            else
                return null;
        }
    }
}
