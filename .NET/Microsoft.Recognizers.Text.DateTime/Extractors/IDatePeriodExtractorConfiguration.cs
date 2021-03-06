﻿ using System.Collections.Generic;
using System.Text.RegularExpressions;

 using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Recognizers.Text.DateTime
{
    public interface IDatePeriodExtractorConfiguration : IOptionsConfiguration
    {
        int MinYearNum { get; }

        int MaxYearNum { get; }

        IEnumerable<Regex> SimpleCasesRegexes { get; }

        Regex YearRegex { get; }

        Regex TillRegex { get; }

        Regex DateUnitRegex { get; }

        Regex FollowedDateUnit { get; }

        Regex NumberCombinedWithDateUnit { get; }

        Regex PastRegex { get; }

        Regex FutureRegex { get; }

        Regex WeekOfRegex { get; }

        Regex MonthOfRegex { get; }

        Regex RangeUnitRegex { get; }

        Regex InConnectorRegex { get; }

        Regex YearPeriodRegex { get; }

        IDateTimeExtractor DatePointExtractor { get; }

        IExtractor CardinalExtractor { get; }

        IDateTimeExtractor DurationExtractor { get; }

        IParser NumberParser { get; }

        bool GetFromTokenIndex(string text, out int index);

        bool HasConnectorToken(string text);

        bool GetBetweenTokenIndex(string text, out int index);
    }
}