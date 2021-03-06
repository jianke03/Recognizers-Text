﻿// ReSharper disable InconsistentNaming

namespace Microsoft.Recognizers.Text.DateTime
{
    public static class Constants
    {
        public const string SYS_DATETIME_DATE = "date";
        public const string SYS_DATETIME_TIME = "time";
        public const string SYS_DATETIME_DATEPERIOD = "daterange";
        public const string SYS_DATETIME_DATETIME = "datetime";
        public const string SYS_DATETIME_TIMEPERIOD = "timerange";
        public const string SYS_DATETIME_DATETIMEPERIOD = "datetimerange";
        public const string SYS_DATETIME_DURATION = "duration";
        public const string SYS_DATETIME_SET = "set";
        public const string SYS_DATETIME_DATETIMEALT = "datetimealt";

        //key
        public const string TimexKey = "timex";
        public const string CommentKey = "Comment";
        public const string ModKey = "Mod";
        public const string TypeKey = "type";
        public const string IsLunarKey = "isLunar";
        public const string ResolveKey = "resolve";
        public const string ResolveToPastKey = "resolveToPast";
        public const string ResolveToFutureKey = "resolveToFuture";
        public const string MultipleDurationType = "multipleDurationType";
        public const string Context = "context";
        public const string SubType = "subType";
        public const string ParentText = "parentText";

        //Multiple Duration Types
        public const string MultipleDuration_DateTime = "multipleDurationDateTime";
        public const string MultipleDuration_Date = "multipleDurationDate";
        public const string MultipleDuration_Time = "multipleDurationTime";

        //Invalid year
        public const int InvalidYear = int.MinValue;
        
        // special value for timezone
        public const int Illegal_minutes = -10000;
        public const string UTCSHIFT = "UTC MINUTES SHIFT";
        public const string SYS_DATETIME_TIMEZONE = "timezone";
    }
}