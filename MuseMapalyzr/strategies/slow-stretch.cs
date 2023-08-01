namespace MuseMapalyzr
{
    public class SlowStretchCheckSegment : CheckSegmentStrategy
    {
        public SlowStretchCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
        {
            if (!Pattern.IsActive)
                return false;

            var previousSegment = Pattern.Segments.Any() ? Pattern.Segments.Last() : null;

            if (currentSegment.SegmentName.Contains("Interval") &&
                (previousSegment == null || previousSegment.SegmentName.Contains("Interval")))
            {
                Pattern.Segments.Add(currentSegment);
                return true;
            }
            return false;
        }
    }

    public class SlowStretchIsAppendable : IsAppendableStrategy
    {
        public SlowStretchIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            if (Pattern.Segments.Count >= 2)
            {
                foreach (var p in Pattern.Segments)
                {
                    if (!p.SegmentName.Contains("Interval"))
                        throw new Exception($"Slow Stretch has a: {p.SegmentName}!!");
                }
                return true;
            }
            return false;

        }
    }
    public class SlowStretchCalcVariation : CalcVariationScoreStrategy
    {
        public SlowStretchCalcVariation(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            // Variation score for Slow Stretches is based on column variation rather than segment variation
            List<int> lst = new List<int>();
            HashSet<double> uniqueSampleTimes = new HashSet<double>();

            foreach (var p in Pattern.Segments)
            {
                foreach (var note in p.Notes)
                {
                    if (!uniqueSampleTimes.Contains(note.SampleTime))
                    {
                        lst.Add(note.Lane);
                        uniqueSampleTimes.Add(note.SampleTime);
                    }
                }
            }

            int n = lst.Count;
            HashSet<int> uniqueVals = new HashSet<int>(lst);
            List<double> freq = uniqueVals.Select(x => (double)lst.Count(i => i == x) / n).ToList();

            double entropy = -freq.Sum(p => p * Math.Log2(p));

            return (int)entropy == 0 ? 1 : entropy;
        }
    }

    public class SlowStretchPatternMultiplier : DefaultCalcPatternMultiplier
    {
        public SlowStretchPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier()
        {
            return base.CalcPatternMultiplier();
        }
    }
}
