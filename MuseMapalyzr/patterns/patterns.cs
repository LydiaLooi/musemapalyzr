using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseMapalyzr
{
    public class Pattern
    {
        public string PatternName { get; set; }
        public List<Segment> Segments { get; set; }
        public double StartSample
        {
            get
            {
                return Segments.First().Notes.First().SampleTime;
            }
        }
        public double EndSample
        {
            get
            {
                return Segments.Last().Notes.Last().SampleTime;
            }
        }
        public bool IsActive { get; set; }
        public int SampleRate { get; set; }
        public int Tolerance { get; set; }
        public double VariationWeighting { get; set; }
        public double PatternWeighting { get; set; }
        public Dictionary<string, double> Intervals { get; set; }
        public double EndExtraDebuff { get; set; }
        public CheckSegmentStrategy CheckSegmentStrategy { get; set; }
        public IsAppendableStrategy IsAppendableStrategy { get; set; }
        public CalcVariationScoreStrategy CalcVariationScoreStrategy { get; set; }
        public CalcPatternMultiplierStrategy CalcPatternMultiplierStrategy { get; set; }
        public CalcPatternLengthMultiplierStrategy CalcPatternLengthMultiplierStrategy { get; set; }

        private int? _totalNotes;

        public int TotalNotes
        {
            get
            {
                if (_totalNotes.HasValue)
                {
                    return _totalNotes.Value;
                }

                int count = 0;
                HashSet<int> uniqueTimestamps = new HashSet<int>();

                foreach (Segment segment in Segments)
                {
                    foreach (Note note in segment.Notes)
                    {
                        if (!uniqueTimestamps.Contains((int)note.SampleTime))
                        {
                            count++;
                            uniqueTimestamps.Add((int)note.SampleTime);
                        }
                    }
                }

                _totalNotes = count;
                return _totalNotes.Value;
            }
        }


        public bool HasIntervalSegment
        {
            get
            {
                foreach (var seg in Segments)
                {
                    if (seg.SegmentName.Contains("Interval"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Pattern(string patternName, List<Segment> segments, int sampleRate)
        {
            PatternName = patternName;
            Segments = segments;

            IsActive = true;
            if (sampleRate == 0)
            {
                SampleRate = Constants.DefaultSampleRate;
            }
            else
            {
                SampleRate = sampleRate;
            }
            Tolerance = ConfigReader.GetConfig().PatternToleranceMs * SampleRate / 1000; ;
            VariationWeighting = ConfigReader.GetConfig().DefaultVariationWeighting;
            PatternWeighting = ConfigReader.GetConfig().DefaultPatternWeighting;
            Intervals = new Dictionary<string, double>()
            {
                {Constants.ShortInterval, ConfigReader.GetConfig().ShortIntDebuff},
                {Constants.MedInterval, ConfigReader.GetConfig().MedIntDebuff},
                {Constants.LongInterval, ConfigReader.GetConfig().LongIntDebuff},
            };
            EndExtraDebuff = ConfigReader.GetConfig().ExtraIntEndDebuff;
            CheckSegmentStrategy = null;
            IsAppendableStrategy = null;
            CalcVariationScoreStrategy = null;
            CalcPatternMultiplierStrategy = null;
            CalcPatternLengthMultiplierStrategy = null;
        }

        public virtual Pattern CreateCopy()
        {
            return new Pattern(PatternName, Segments, SampleRate);
        }

        public Dictionary<string, int> GetSegmentTypeCounts(List<string> segmentNames)
        {
            var segmentCounts = new Dictionary<string, int>
        {
            { Constants.Switch, 0 },
            { Constants.ZigZag, 0 },
            { Constants.TwoStack, 0 },
            { Constants.ThreeStack, 0 },
            { Constants.FourStack, 0 },
            { Constants.SingleStreams, 0 },
            { Constants.ShortInterval, 0 },
            { Constants.MedInterval, 0 },
            { Constants.LongInterval, 0 },
        };

            foreach (var name in segmentNames)
            {
                if (segmentCounts.ContainsKey(name))
                {
                    segmentCounts[name]++;
                }
            }

            return segmentCounts;
        }
        public double CalcSwitchDebuff(Dictionary<string, int> segmentCounts, double entropy)
        {
            double? switchProportion;
            var switchCount = segmentCounts[Constants.Switch];
            var totalPatterns = segmentCounts.Values.Sum();
            if (entropy > 1 && switchCount > 0)
            {
                double switchDebuff;
                if (totalPatterns < 4)
                {
                    switchDebuff = 0.7;
                }
                else
                {
                    switchProportion = (double)switchCount / totalPatterns;
                    switchDebuff = switchProportion < 0.5 ? 0.8 : 0.9; // if there are more switches, then don't make the buff as hard
                }

                // Log the switch proportion and debuff.
                entropy *= switchDebuff;
            }

            return entropy;
        }

        public bool IsNStack(Segment segment)
        {
            List<string> stackNames = new List<string> {
                Constants.TwoStack,
                Constants.ThreeStack,
                Constants.FourStack
                };
            return stackNames.Contains(segment.SegmentName);
        }

        public bool SegmentIsInterval(Segment? segment)
        {
            if (segment != null)
            {
                return segment.SegmentName.Contains("Interval");
            }
            return false;
        }

        public bool TimeDifferenceIsTolerable(Segment previousSegment, Segment currentSegment)
        {
            if (previousSegment.TimeDifference == null || currentSegment.TimeDifference == null)
                throw new InvalidOperationException("Time difference cannot be null.");

            if (SegmentIsInterval(previousSegment) || SegmentIsInterval(currentSegment))
                return true;

            var result = Math.Abs(currentSegment.TimeDifference.Value - previousSegment.TimeDifference.Value) <= Tolerance;

            return result;
        }

        public bool IntervalBetweenSegmentsIsTolerable(Segment previousSegment, Segment currentSegment)
        {
            if (previousSegment.Notes.Count <= 1 || currentSegment.Notes.Count <= 1)
                throw new InvalidOperationException("Segment must contain more than one note.");

            if (SegmentIsInterval(previousSegment) || SegmentIsInterval(currentSegment))
                return true;

            var endOfFirst = previousSegment.Notes.Last().SampleTime;
            var startOfSecond = currentSegment.Notes.First().SampleTime;
            var timeDifference = Math.Abs(endOfFirst - startOfSecond);

            return timeDifference <= Tolerance;
        }

        public bool AddIntervalIsAtStart(Segment intervalSegment)
        {
            if (Segments.Count == 0)
            {
                Segments.Add(intervalSegment);
                return true;
            }
            else
            {
                Segments.Add(intervalSegment);
                return false;
            }
        }

        public void ResetGroup(Segment previousSegment, Segment currentSegment)
        {
            IsActive = true;
            Segments = new List<Segment>();

            // If previous or currentSegment is an Interval, then only add that one (prioritise the latest one).
            if (SegmentIsInterval(currentSegment))
            {
                CheckSegment(currentSegment);
            }
            else if (SegmentIsInterval(previousSegment))
            {
                CheckSegment(previousSegment);
                CheckSegment(currentSegment);
            }
            // If not, then attempt to add the previous one first, then the current one too
            // If it fails at any point, set the group to inactive
            else
            {
                if (previousSegment != null)
                {
                    bool added = CheckSegment(previousSegment).Value;
                    if (added)
                    {
                        added = CheckSegment(currentSegment).Value;
                    }

                    if (!added)
                    {
                        IsActive = false;
                    }
                }
            }
        }

        public void CalculatePatternDifficulty(bool ranked)
        {

            double variationMultiplier = CalcVariationScore();
            double patternMultiplier = CalcPatternMultiplier(ranked);

            double final = (VariationWeighting * variationMultiplier) + (PatternWeighting * patternMultiplier);

            //Console.WriteLine($"Ranked {ranked} | Final: {final} | Pattern Multiplier {patternMultiplier} | Pattern Weighting {PatternWeighting} | Variation Multiplier {variationMultiplier} | Variation Weighting {VariationWeighting}");


            if (PatternName != Constants.Other) SetSegmentsMultiplier(final, ranked);
        }


        public void SetCheckSegmentStrategy(CheckSegmentStrategy strategy)
        {
            CheckSegmentStrategy = strategy;
        }

        public void SetIsAppendableStrategy(IsAppendableStrategy strategy)
        {
            IsAppendableStrategy = strategy;
        }

        public void SetCalcVariationScoreStrategy(CalcVariationScoreStrategy strategy)
        {
            CalcVariationScoreStrategy = strategy;
        }

        public void SetCalcPatternMultiplierStrategy(CalcPatternMultiplierStrategy strategy)
        {
            CalcPatternMultiplierStrategy = strategy;
        }

        public void SetCalcPatternLengthMultiplierStrategy(CalcPatternLengthMultiplierStrategy strategy)
        {
            CalcPatternLengthMultiplierStrategy = strategy;
        }

        // Use strategy pattern to delegate method calls
        public bool? CheckSegment(Segment? currentSegment)
        {
            return CheckSegmentStrategy.CheckSegment(currentSegment);
        }

        public bool IsAppendable()
        {
            return IsAppendableStrategy.IsAppendable();
        }

        public double CalcVariationScore()
        {
            return CalcVariationScoreStrategy.CalcVariationScore();
        }

        public double CalcPatternMultiplier(bool ranked)
        {
            return CalcPatternMultiplierStrategy.CalcPatternMultiplier(ranked);
        }

        public double CalcPatternLengthMultiplier()
        {
            return CalcPatternLengthMultiplierStrategy.CalcPatternLengthMultiplier();
        }

        public void SetSegmentsMultiplier(double multiplier, bool ranked)
        {
            CalcPatternMultiplierStrategy.SetSegmentsMultiplier(multiplier, ranked);
        }

        public override string ToString()
        {
            return $"{PatternName} Segments: {Segments.Count}";
        }

    }

}