namespace MuseMapalyzr
{
    public class Pattern
    {
        public string PatternName { get; set; }
        public List<Segment> Segments { get; set; }
        public int StartSample { get; set; }
        public int EndSample { get; set; }
        public bool IsActive { get; set; }
        public int SampleRate { get; set; }
        public int Tolerance { get; set; }
        public double VariationWeighting { get; set; }
        public double PatternWeighting { get; set; }
        public Dictionary<string, double> Intervals { get; set; }
        public double EndExtraDebuff { get; set; }
        public object CheckSegmentStrategy { get; set; }
        public object IsAppendableStrategy { get; set; }
        public object CalcVariationScoreStrategy { get; set; }
        public object CalcPatternMultiplierStrategy { get; set; }
        public object CalcPatternLengthMultiplierStrategy { get; set; }
        public int? TotalNotes { get; set; }

        public Pattern(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
        {
            PatternName = patternName;
            Segments = segments;
            StartSample = startSample;
            EndSample = endSample;
            IsActive = true;
            SampleRate = sampleRate;
            Tolerance = 0; // Initialize with your value
            VariationWeighting = 0; // Initialize with your value
            PatternWeighting = 0; // Initialize with your value
            Intervals = new Dictionary<string, double>(); // Initialize with your values
            EndExtraDebuff = 0; // Initialize with your value
            CheckSegmentStrategy = null;
            IsAppendableStrategy = null;
            CalcVariationScoreStrategy = null;
            CalcPatternMultiplierStrategy = null;
            CalcPatternLengthMultiplierStrategy = null;
            TotalNotes = null;
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
            double? switchProportion = null;
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
                Console.WriteLine($">>> Switch (proportion {switchProportion}) debuff by {switchDebuff:.2f} <<<");
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

        public bool SegmentIsInterval(Segment segment)
        {
            return segment.SegmentName.Contains("Interval");
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

    }

    public class OtherPattern
    {
        // Assuming OtherPattern is a class. Define it according to your needs.
        // You need to provide the constructor for OtherPattern similar to Python's OtherPattern(OTHER, []).

        public OtherPattern(string other, List<Pattern> list)
        {
            // Initialize the OtherPattern object here.
        }
    }
}