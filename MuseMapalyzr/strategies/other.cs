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

        public override double CalcPatternMultiplier()
        {
            // Other Patterns are calculated based on the weighted average
            // of the segment difficulties within the Other pattern.
            List<double> multipliers = new List<double>();

            foreach (var segment in Pattern.Segments)
            {
                switch (segment.SegmentName)
                {
                    case Constants.Switch:
                        multipliers.Add(double.Parse(ConfigReader.GetConfig()["other_switch_multiplier"]));
                        break;
                    case Constants.ZigZag:
                        multipliers.Add(PatternMultiplier.ZigZagMultiplier(segment.NotesPerSecond)); // assuming you have a method for this
                        break;
                    case Constants.TwoStack:
                        multipliers.Add(PatternMultiplier.TwoStackMultiplier(segment.NotesPerSecond)); // assuming you have a method for this
                        break;
                    case Constants.ThreeStack:
                        multipliers.Add(PatternMultiplier.ThreeStackMultiplier(segment.NotesPerSecond)); // assuming you have a method for this
                        break;
                    case Constants.FourStack:
                        multipliers.Add(PatternMultiplier.FourStackMultiplier(segment.NotesPerSecond)); // assuming you have a method for this
                        break;
                    case Constants.SingleStreams:
                        multipliers.Add(PatternMultiplier.StreamMultiplier(segment.NotesPerSecond)); // assuming you have a method for this
                        break;
                    case Constants.ShortInterval:
                        multipliers.Add(double.Parse(ConfigReader.GetConfig()["other_short_int_multiplier"]));
                        break;
                    case Constants.MedInterval:
                        multipliers.Add(double.Parse(ConfigReader.GetConfig()["other_med_int_multiplier"]));
                        break;
                    case Constants.LongInterval:
                        multipliers.Add(double.Parse(ConfigReader.GetConfig()["other_long_int_multiplier"]));
                        break;
                    default:
                        // Console.WriteLine($"WARNING: Did not recognise pattern: {segment.SegmentName}");
                        multipliers.Add(1);
                        break;
                }
            }

            Console.WriteLine($"Other Pattern Multipliers: [{string.Join(", ", multipliers)}]");
            double weightedAverage = DifficultyCalculation.WeightedAverageOfValues(multipliers); // assuming you have a method for this

            return weightedAverage;
        }
    }
}
