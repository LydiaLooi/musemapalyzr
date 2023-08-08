namespace MuseMapalyzr
{
    public class OtherCheckSegment : CheckSegmentStrategy
    {
        public OtherCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
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

            foreach (Segment segment in Pattern.Segments)
            {
                switch (segment.SegmentName)
                {
                    case Constants.Switch:
                        double switchMultiplier = double.Parse(ConfigReader.GetConfig()["other_switch_multiplier"]);
                        // Console.WriteLine($"Switch: {switchMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(switchMultiplier);
                        break;
                    case Constants.ZigZag:
                        // Zig zags are special as they can have many notes in them.
                        double zigZagMultiplier = PatternMultiplier.ZigZagMultiplier(segment.NotesPerSecond);
                        double zigZagLengthMultiplier = PatternMultiplier.ZigZagLengthMultiplier(segment.Notes.Count, segment.NotesPerSecond);
                        // Console.WriteLine($"ZIG ZAG MULTIPLIER: {multiplier} * {extra} = {multiplier * extra}");
                        // Console.WriteLine($"Zig Zag: {zigZagMultiplier * zigZagLengthMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(zigZagMultiplier * zigZagLengthMultiplier);
                        break;
                    case Constants.TwoStack:
                        double twoStackMultiplier = PatternMultiplier.TwoStackMultiplier(segment.NotesPerSecond);
                        // Console.WriteLine($"Two Stack: {twoStackMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(twoStackMultiplier);
                        break;
                    case Constants.ThreeStack:
                        double threeStackMultiplier = PatternMultiplier.ThreeStackMultiplier(segment.NotesPerSecond);
                        // Console.WriteLine($"Three Stack: {threeStackMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(threeStackMultiplier);
                        break;
                    case Constants.FourStack:
                        double fourStackMultiplier = PatternMultiplier.FourStackMultiplier(segment.NotesPerSecond);
                        // Console.WriteLine($"Four Stack: {fourStackMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(fourStackMultiplier);
                        break;
                    case Constants.SingleStreams:
                        double streamMultiplier = PatternMultiplier.StreamMultiplier(segment.NotesPerSecond);
                        // Console.WriteLine($"Stream multiplier: {streamMultiplier,-40} NPS: {segment.NotesPerSecond}");
                        multipliers.Add(streamMultiplier);
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
                        // // Console.WriteLine($"WARNING: Did not recognise pattern: {segment.SegmentName}");
                        multipliers.Add(1);
                        break;
                }
            }

            // Console.WriteLine($"Other Pattern Multipliers: [{string.Join(", ", multipliers)}]");

            // If other has quite a few patterns and notes, then higher weighting to the harder patterns
            double weightedAverage;
            if (multipliers.Count > 5 || Pattern.TotalNotes > 20)
            {
                weightedAverage = DifficultyCalculation.WeightedAverageOfValues(multipliers, 0.3, 0.9, 0.1); // assuming you have a method for this
            }
            else
            {
                weightedAverage = DifficultyCalculation.WeightedAverageOfValues(multipliers, 0.3, 0.6, 0.4); // assuming you have a method for this
            }
            // Console.WriteLine("Other Multiplier Count: " + multipliers.Count);
            // Console.WriteLine($"Other Pattern Multiplier Weighted Average: {weightedAverage}");
            // if (multipliers.Count > 10)
            // {
            //     Console.WriteLine($"Other Pattern Multipliers: [{string.Join(", ", multipliers)}]");
            // }

            return weightedAverage;
        }
    }
}
