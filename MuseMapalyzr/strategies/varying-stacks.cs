using System;
using System.Collections.Generic;

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

        public override double CalcPatternMultiplier(bool ranked)
        {

            // Note: This is almost same as the Other pattern calc. It should only need
            // the n-stack

            // Intervals are just set to 1 since they should only be at the start and end.
            List<double> multipliers = new List<double>();

            var conf = ConfigReader.GetConfig();
            if (!ranked)
            {
                conf = ConfigReader.GetUnrankedConfig();
            }

            double varyingStacksMultiplier = conf.VaryingStackMultiplier;

            foreach (Segment segment in Pattern.Segments)
            {
                double multiplier = 1;



                switch (segment.SegmentName)
                {
                    case Constants.Switch:
                        multiplier = conf.OtherSwitchMultiplier;
                        break;
                    case Constants.TwoStack:
                        multiplier = PatternMultiplier.TwoStackMultiplier(segment.NotesPerSecond, ranked) * varyingStacksMultiplier;
                        break;
                    case Constants.ThreeStack:
                        multiplier = PatternMultiplier.ThreeStackMultiplier(segment.NotesPerSecond, ranked) * varyingStacksMultiplier;
                        break;
                    case Constants.FourStack:
                        multiplier = PatternMultiplier.FourStackMultiplier(segment.NotesPerSecond, ranked) * varyingStacksMultiplier;
                        break;
                    case Constants.SingleStreams:
                        multiplier = PatternMultiplier.StreamMultiplier(segment.NotesPerSecond, ranked) * varyingStacksMultiplier;
                        break;
                    default:
                        multipliers.Add(multiplier);
                        break;
                }

                multipliers.Add(multiplier);
                if (ranked)
                {
                    segment.RankedMultiplier = multiplier;

                }
                else
                {
                    segment.UnrankedMultiplier = multiplier;
                }

            }

            // If other has quite a few patterns and notes, then higher weighting to the harder patterns
            double weightedAverage;
            if (multipliers.Count > 5 || Pattern.TotalNotes > 20) // Hard coded for now. Numbers are kinda arbitrary.
            {
                weightedAverage = Utils.WeightedAverageOfValues(multipliers, 0.3, 0.9, 0.1);
            }
            else
            {
                weightedAverage = Utils.WeightedAverageOfValues(multipliers, 0.3, 0.6, 0.4);
            }
            return weightedAverage;
        }
    }
}
