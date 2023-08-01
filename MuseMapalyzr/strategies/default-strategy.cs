namespace MuseMapalyzr
{
    public class DefaultCalcVariationScore : CalcVariationScoreStrategy
    {
        public DefaultCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            // If there are no segments, return 0
            if (this.Pattern.Segments.Count == 0)
            {
                return 0;
            }

            List<string> tempLst = this.Pattern.Segments.Select(s => s.SegmentName).ToList();
            List<double> intervalList = new List<double>();
            List<string> segmentNames = new List<string>();

            Dictionary<string, int> patternCounts = this.Pattern.GetSegmentTypeCounts(tempLst);

            // Check for intervals
            for (int i = 0; i < tempLst.Count; i++)
            {
                string name = tempLst[i];
                if (this.Pattern.Intervals.ContainsKey(name))
                {
                    if (i == 0 || i == tempLst.Count - 1)
                    {
                        intervalList.Add(this.Pattern.Intervals[name] * this.Pattern.EndExtraDebuff);
                    }
                    else
                    {
                        intervalList.Add(this.Pattern.Intervals[name]);
                        segmentNames.Add("Interval");
                    }
                }
                else
                {
                    segmentNames.Add(name);
                }
            }

            Console.WriteLine($"Checking entropy of: {string.Join(", ", segmentNames)}");

            double n = segmentNames.Count;
            var freq = segmentNames.GroupBy(x => x)
                                   .ToDictionary(g => g.Key, g => g.Count() / n)
                                   .Values;

            double entropy = -freq.Sum(p => p * Math.Log2(p));

            if (intervalList.Count != 0)
            {
                double averageDebuff = intervalList.Average();
                entropy *= averageDebuff;

                // Console.WriteLine($">>> Debuffing (due to Intervals) by {averageDebuff} <<<");
            }

            entropy = this.Pattern.CalcSwitchDebuff(patternCounts, entropy);

            if (entropy == 0)
            {
                return 1;
            }

            return entropy;
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

        public override bool? CheckSegment(Segment? currentSegment)
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