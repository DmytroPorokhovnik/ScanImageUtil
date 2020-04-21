using ScanImageUtil.Back.Models;
using System;

namespace ScanImageUtil.Back.Extensions
{
    internal static class EnumModelsExtension
    {
        public static string ToRussianString(this WorkResult workResult)
        {
            switch (workResult)
            {
                case WorkResult.Late:
                    return "Выполнено в срок";
                case WorkResult.OnTime:
                    return "Выполнено не в срок";
                case WorkResult.None:
                    return "";
                default:
                    throw new Exception($"No such value '{workResult}' in workResult enum");
            }
        }

        public static string ToRussianString(this Summary summary)
        {
            switch (summary)
            {
                case Summary.DoneOnTime:
                    return "Выполнено";
                case Summary.DoneLate:
                    return "Не выполнено в срок";
                case Summary.NotDone:
                    return "Не выполнено";
                case Summary.None:
                    return "";
                default:
                    throw new Exception($"No such value '{summary}' in workResult enum");
            }
        }

        public static string ToRussianString(this WorkType workType)
        {
            switch (workType)
            {
                case WorkType.Maintenance:
                    return "ТО";
                case WorkType.Repair:
                    return "Ремонт";
                case WorkType.SoftWare:
                    return "ПО";
                case WorkType.MaintenanceAndSoftware:
                    return "ТО ПО";
                case WorkType.None:
                    return "";
                default:
                    throw new Exception($"No such value '{workType}' in workResult enum");
            }
        }
    }
}
