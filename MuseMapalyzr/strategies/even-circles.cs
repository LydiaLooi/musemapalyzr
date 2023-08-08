namespace MuseMapalyzr
{
    public class EvenCirclesCheckSegment : CheckSegmentStrategy
    {
        public EvenCirclesCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
        {
            if (!Pattern.IsActive)
                return false;

            Segment? previousSegment = Pattern.Segments.Any() ? Pattern.Segments.Last() : null;

            if (Pattern.SegmentIsInterval(currentSegment))
            {
                bool atStart = Pattern.AddIntervalIsAtStart(currentSegment);
                if (!atStart)
                    return false;
                return true;
            }

            // Check for invalid combinations of previous segment and current segment
            if (!Pattern.IsNStack(currentSegment) && currentSegment.SegmentName != Constants.Switch)
                return false;

            if (previousSegment != null)
            {
                // If the previous segment is an interval, then we can add it.
                if (Pattern.SegmentIsInterval(previousSegment))
                {
                    Pattern.Segments.Add(currentSegment);
                    return true;
                }

                if (previousSegment.SegmentName == Constants.Switch && !Pattern.IsNStack(currentSegment))
                    return false;

                if (Pattern.IsNStack(previousSegment) && currentSegment.SegmentName != Constants.Switch)
                    return false;

                if (!Pattern.TimeDifferenceIsTolerable(previousSegment, currentSegment))
                    return false;

                if (!Pattern.IntervalBetweenSegmentsIsTolerable(previousSegment, currentSegment))
                    return false;
            }

            // Current segment should be valid from here
            Pattern.Segments.Add(currentSegment);
            return true;
        }
    }

    public class EvenCirclesIsAppendable : IsAppendableStrategy
    {
        public EvenCirclesIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            if (Pattern.Segments.Count >= 3)
            {
                // Sanity check that everything in it is only N-stacks or Switches
                int nStackCount = 0;
                foreach (Segment p in Pattern.Segments)
                {
                    if (Pattern.IsNStack(p))
                        nStackCount += 1;
                    if (!Pattern.IsNStack(p) && p.SegmentName != Constants.Switch && !Pattern.SegmentIsInterval(p))
                        throw new ArgumentException($"Even Circle has a: {p.SegmentName}!!");
                }
                if (nStackCount >= 2)  // There must be at least 2 n_stacks to be valid
                    return true;
            }
            return false;
        }
    }

    public class EvenCirclesCalcVariationScore : DefaultCalcVariationScore
    {
        public EvenCirclesCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            double variationScore = base.CalcVariationScore();

            // Make a minor change to the return value
            double modifiedVariationScore = Math.Max(1, variationScore);

            return modifiedVariationScore;
        }
    }

    public class EvenCirclesCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public EvenCirclesCalcPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier()
        {
            double nps = Pattern.Segments[0].NotesPerSecond;  // Even Circle should have consistent NPS
            double multiplier = PatternMultiplier.EvenCircleMultiplier(nps);  // You'll need to define the EvenCircleMultiplier method
            // Console.WriteLine($"EvenCirclesCalcPatternMultiplier: {multiplier}");
            return multiplier;
        }
    }

}