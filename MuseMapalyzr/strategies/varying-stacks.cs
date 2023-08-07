namespace MuseMapalyzr
{
    public class VaryingStacksCheckSegment : CheckSegmentStrategy
    {
        public VaryingStacksCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
        {
            if (!Pattern.IsActive)
                return false;

            if (Pattern.SegmentIsInterval(currentSegment))
            {
                var atStart = Pattern.AddIntervalIsAtStart(currentSegment);
                if (!atStart)
                    return false;
                return true;
            }

            // Check if current segment is straight up invalid
            if (!Pattern.IsNStack(currentSegment))
                return false;

            // Current segment should be valid from here
            Pattern.Segments.Add(currentSegment);
            return true;
        }
    }

    public class VaryingStacksIsAppendable : IsAppendableStrategy
    {
        public VaryingStacksIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            if (Pattern.Segments.Count >= 2)
            {
                // Needs at least 2 n-stacks to be valid
                int nStackCount = 0;
                foreach (var p in Pattern.Segments)
                {
                    if (Pattern.IsNStack(p))
                        nStackCount++;
                    if (!Pattern.IsNStack(p) && !Pattern.SegmentIsInterval(p))
                        throw new Exception($"Varying Stack has a: {p.SegmentName}!!");
                }
                if (nStackCount >= 2)
                    return true;
            }
            return false;
        }
    }

    public class VaryingStacksCalcVariationScore : DefaultCalcVariationScore
    {
        public VaryingStacksCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            double variationScore = base.CalcVariationScore();

            // Make a minor change to the return value
            double modifiedVariationScore = Math.Max(1, variationScore);

            return modifiedVariationScore;
        }
    }

    public class VaryingStacksCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public VaryingStacksCalcPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier()
        {
            double nps = Pattern.Segments[0].NotesPerSecond;
            double multiplier = PatternMultiplier.VaryingStacksMultiplier(nps); // assuming you have a method for this
            Console.WriteLine($"VaryingStacksCalcPatternMultiplier: {multiplier}");
            return multiplier;
        }
    }
}
