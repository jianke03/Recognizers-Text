﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DateObject = System.DateTime;

using Microsoft.Recognizers.Text.Number;

namespace Microsoft.Recognizers.Text.DateTime
{
    public class BaseMergedExtractor : IDateTimeExtractor
    {
        private readonly IMergedExtractorConfiguration config;

        public BaseMergedExtractor(IMergedExtractorConfiguration config)
        {
            this.config = config;
        }

        public List<ExtractResult> Extract(string text)
        {
            return Extract(text, DateObject.Now);
        }

        public List<ExtractResult> Extract(string text, DateObject reference)
        {
            var ret = new List<ExtractResult>();

            // The order is important, since there is a problem in merging
            AddTo(ret, this.config.DateExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.TimeExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.DurationExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.DatePeriodExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.DateTimeExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.TimePeriodExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.DateTimePeriodExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.SetExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.HolidayExtractor.Extract(text, reference), text);
            AddTo(ret, this.config.TimeZoneExtractor.Extract(text, reference), text);

            // This should be at the end since if need the extractor to determine the previous text contains time or not
            AddTo(ret, NumberEndingRegexMatch(text, ret), text);

            // modify time entity to an alternative DateTime expression if it follows a DateTime entity
            if ((this.config.Options & DateTimeOptions.ExtendedTypes) != 0)
            {
                ret = this.config.DateTimeAltExtractor.Extract(ret, text, reference);
            }

            AddMod(ret, text);

            // filtering
            if ((this.config.Options & DateTimeOptions.CalendarMode) != 0)
            {
                CheckCalendarFilterList(ret, text);
            }

            ret = ret.OrderBy(p => p.Start).ToList();

            return ret;
        }

        private void CheckCalendarFilterList(List<ExtractResult> ers, string text)
        {
            foreach (var er in ers.Reverse<ExtractResult>())
            {
                foreach (var negRegex in this.config.FilterWordRegexList)
                {
                    var match = negRegex.Match(er.Text);
                    if (match.Success)
                    {
                        ers.Remove(er);
                    }
                }
            }
        }

        private void AddTo(List<ExtractResult> dst, List<ExtractResult> src, string text)
        {
            foreach (var result in src)
            {
                if ((config.Options & DateTimeOptions.SkipFromToMerge) != 0)
                {
                    if (ShouldSkipFromToMerge(result))
                    {
                        continue;
                    }
                }

                // comment this code for now
                //if (FilterAmbiguousSingleWord(result, text))
                //{
                //    continue;
                //}

                var isFound = false;
                List<int> overlapIndexes=new List<int>();
                int firstIndex = -1;
                for (var i = 0; i < dst.Count; i++)
                {
                    //if (dst[i].IsOverlap(result))
                    //{
                    //    if (firstIndex == -1)
                    //    {
                    //        firstIndex = i;
                    //    }
                    //    isFound = true;
                    //    if (result.Length > dst[i].Length)
                    //    {
                    //        overlapIndexes.Add(i);
                    //    }
                    //}
                    if (dst[i].IsOverlap(result))
                    {
                        isFound = true;
                        if (dst[i].IsCover(result))
                        {
                            if (firstIndex == -1)
                            {
                                firstIndex = i;
                            }
                            overlapIndexes.Add(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (!isFound)
                {
                    dst.Add(result);
                }
                else if (overlapIndexes.Count>0)
                {
                    var tempDst = new List<ExtractResult>();
                    for (var i = 0; i < dst.Count; i++)
                    {
                        if (!overlapIndexes.Contains(i))
                        {
                            tempDst.Add(dst[i]);
                        }
                    }

                    //insert at the first overlap occurence to keep the order
                    tempDst.Insert(firstIndex, result);
                    dst.Clear();
                    dst.AddRange(tempDst);
                }
            }
        }

        private bool ShouldSkipFromToMerge(ExtractResult er) {
            return config.FromToRegex.IsMatch(er.Text);
        }

        private bool FilterAmbiguousSingleWord(ExtractResult er, string text)
        {
            if (config.SingleAmbiguousMonthRegex.IsMatch(er.Text.ToLowerInvariant()))
            {
                var stringBefore = text.Substring(0, (int) er.Start).TrimEnd();
                if (!config.PrepositionSuffixRegex.IsMatch(stringBefore))
                {
                    return true;
                }
            }

            return false;
        }

        // handle cases like "move 3pm appointment to 4"
        private List<ExtractResult> NumberEndingRegexMatch(string text, List<ExtractResult> extractResults)
        {
            var tokens = new List<Token>();

            foreach (var extractResult in extractResults)
            {
                if (extractResult.Type.Equals(Constants.SYS_DATETIME_TIME)
                    || extractResult.Type.Equals(Constants.SYS_DATETIME_DATETIME))
                {
                    var stringAfter = text.Substring((int)extractResult.Start + (int)extractResult.Length);
                    var match = this.config.NumberEndingPattern.Match(stringAfter);
                    if (match != null && match.Success)
                    {
                        var newTime = match.Groups["newTime"];
                        var numRes = this.config.IntegerExtractor.Extract(newTime.ToString());
                        if (numRes.Count == 0)
                        {
                            continue;
                        }

                        var startPosition = (int)extractResult.Start + (int)extractResult.Length + newTime.Index;
                        tokens.Add(new Token(startPosition, startPosition + newTime.Length));
                    }
                }
            }

            return Token.MergeAllTokens(tokens, text, Constants.SYS_DATETIME_TIME);
        }

        private void AddMod(List<ExtractResult> ers, string text)
        {
            var lastEnd = 0;
            foreach (var er in ers)
            {
                var beforeStr = text.Substring(lastEnd, er.Start ?? 0).ToLowerInvariant();

                if (HasTokenIndex(beforeStr.TrimEnd(), config.BeforeRegex, out int tokenIndex))
                {
                    var modLengh = beforeStr.Length - tokenIndex;
                    er.Length += modLengh;
                    er.Start -= modLengh;
                    er.Text = text.Substring(er.Start ?? 0, er.Length ?? 0);
                }

                if (HasTokenIndex(beforeStr.TrimEnd(), config.AfterRegex, out tokenIndex))
                {
                    var modLengh = beforeStr.Length - tokenIndex;
                    er.Length += modLengh;
                    er.Start -= modLengh;
                    er.Text = text.Substring(er.Start ?? 0, er.Length ?? 0);
                }

                if (HasTokenIndex(beforeStr.TrimEnd(), config.SinceRegex, out tokenIndex))
                {
                    var modLengh = beforeStr.Length - tokenIndex;
                    er.Length += modLengh;
                    er.Start -= modLengh;
                    er.Text = text.Substring(er.Start ?? 0, er.Length ?? 0);
                }

                if (er.Type.Equals(Constants.SYS_DATETIME_DATEPERIOD))
                {
                    // 2012 or after/above
                    var afterStr = text.Substring((er.Start ?? 0) + (er.Length ?? 0)).ToLowerInvariant();

                    var match = config.YearAfterRegex.Match(afterStr.TrimStart());
                    if (match.Success && match.Index == 0)
                    {
                        var modLengh = match.Length + afterStr.IndexOf(match.Value);
                        er.Length += modLengh;
                        er.Text = text.Substring(er.Start ?? 0, er.Length ?? 0);
                    }
                }
            }
        }

        public bool HasTokenIndex(string text, Regex regex, out int index)
        {
            index = -1;
            var match = regex.Match(text);

            if (match.Success && string.IsNullOrWhiteSpace(text.Substring(match.Index+match.Length)))
            {
                index = match.Index;
                return true;
            }

            return false;
        }
    }
}
