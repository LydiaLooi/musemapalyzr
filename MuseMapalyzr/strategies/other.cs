using System.Collections.Generic;

namespace MuseMapalyzr
{
    public class OtherCheckSegment : CheckSegmentStrategy
    {
        public OtherCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment currentSegment)
        {
            Pattern.Segments.Add(currentSegment);
            return true;
        }
    }

    public class OtherIsAppendable : IsAppendableStrategy
    {
        public OtherIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            return true;
        }
    }

    public class OtherCalcVariationScore : DefaultCalcVariationScore
    {
        public OtherCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            return base.CalcVariationScore();
        }
    }

    public class OtherCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public OtherCalcPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier(bool ranked)
        {
            // Other Patterns are calculated based on the weighted average
            // of the segment difficulties within the Other pattern.
            List<double> multipliers = new List<double>();

            var conf = ConfigReader.GetConfig();
            if (!ranked)
            {
                conf = ConfigReader.GetUnrankedConfig();
            }


            foreach (var segment in Pattern.Segments)
            {
                switch (segment.SegmentName)
                {
                    case Constants.Switch:
                        multipliers.Add(conf.OtherSwitchMultiplier);
                        break;
                    case Constants.ZigZag:
                        // Zig zags are special as they can have many notes in them.
                        double zigZagMultiplier = PatternMultiplier.ZigZagMultiplier(segment.NotesPerSecond, ranked);
                        double zigZagLengthMultiplier = PatternMultiplier.ZigZagLengthMultiplier(segment.Notes.Count, segment.NotesPerSecond, ranked);
                        multipliers.Add(zigZagMultiplier * zigZagLengthMultiplier);
                        break;
                    case Constants.TwoStack:
                        multipliers.Add(PatternMultiplier.TwoStackMultiplier(segment.NotesPerSecond, ranked));
                        break;
                    case Constants.ThreeStack:
                        multipliers.Add(PatternMultiplier.ThreeStackMultiplier(segment.NotesPerSecond, ranked));
                        break;
                    case Constants.FourStack:
                        multipliers.Add(PatternMultiplier.FourStackMultiplier(segment.NotesPerSecond, ranked));
                        break;
                    case Constants.SingleStreams:
                        multipliers.Add(PatternMultiplier.StreamMultiplier(segment.NotesPerSecond, ranked));
                        break;
                    case Constants.ShortInterval:

                        multipliers.Add(conf.OtherShortIntMultiplier);
                        break;
                    case Constants.MedInterval:
                        multipliers.Add(conf.OtherMedIntMultiplier);
                        break;
                    case Constants.LongInterval:
                        multipliers.Add(conf.OtherLongIntMultiplier);
                        break;
                    default:
                        multipliers.Add(1);
                        break;
                }
            }

            // If other has quite a few patterns and notes, then higher weighting to the harder patterns
            double weightedAverage;
            if (multipliers.Count > 5 || Pattern.TotalNotes > 20) // Hard coded for now. Numbers are kinda arbitrary.
            {
                weightedAverage = DifficultyCalculation.WeightedAverageOfValues(multipliers, 0.3, 0.9, 0.1);
            }
            else
            {
                weightedAverage = DifficultyCalculation.WeightedAverageOfValues(multipliers, 0.3, 0.6, 0.4);
            }
            return weightedAverage;
        }
    }
}
