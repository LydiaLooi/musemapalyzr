namespace MuseMapalyzr
{
    public class DefaultCalcVariationScore : CalcVariationScoreStrategy
    {
        public DefaultCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            if (!Pattern.Segments.Any())
                return 0;

            var tempLst = Pattern.Segments.Select(s => s.SegmentName).ToList();
            var intervalList = new List<double>();
            var segmentNames = new List<string>();

            var patternCounts = Pattern.GetSegmentTypeCounts(tempLst);

            // Check for intervals:
            // TODO: Implement the logic here.

            var n = segmentNames.Count;
            var freq = segmentNames.GroupBy(x => x).Select(g => (double)g.Count() / n);
            var entropy = -freq.Sum(p => p * Math.Log2(p));

            if (intervalList.Any())
            {
                // average interval debuffs and multiply that by the entropy
                var averageDebuff = intervalList.Average();
                entropy *= averageDebuff;

                // Log the debuffing due to Intervals.
            }

            entropy = Pattern.CalcSwitchDebuff(patternCounts, entropy);

            if (entropy == 0)  // Temp?
                return 1;

            return (double)entropy;
        }
    }

    public class DefaultCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public DefaultCalcPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier()
        {
            return 1;
        }
    }

    public class DefaultCalcPatternLengthMultiplier : CalcPatternLengthMultiplierStrategy
    {
        public DefaultCalcPatternLengthMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternLengthMultiplier()
        {
            return 1;
        }
    }

    public class DefaultCheckSegment : CheckSegmentStrategy
    {
        public DefaultCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment currentSegment)
        {
            throw new NotImplementedException("You should use an actual strategy!");
        }
    }

    public class DefaultIsAppendable : IsAppendableStrategy
    {
        public DefaultIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            throw new NotImplementedException("You should use an actual strategy!");
        }
    }
}