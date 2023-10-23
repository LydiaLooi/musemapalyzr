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


            int zigZagChainNoteCount = 0;
            double prevNPS = -1;
            foreach (Segment segment in Pattern.Segments)
            {
                double multiplier = 1;

                // This is to keep track of how many zig zag segments we have seen so we can properly
                // use the zig zag length multiplier
                if (segment.SegmentName == Constants.ZigZag &&
                    (prevNPS == -1 || prevNPS == segment.NotesPerSecond))
                {

                    zigZagChainNoteCount += segment.Notes.Count - 1; // Due to the overlap of segments
                    prevNPS = (double)segment.NotesPerSecond;

                }
                else
                {
                    prevNPS = -1;

                    if (segment.SegmentName == Constants.ZigZag)
                    {
                        zigZagChainNoteCount = segment.Notes.Count - 1;

                    }
                    else
                    {
                        zigZagChainNoteCount = 0;

                    }
                }



                switch (segment.SegmentName)
                {
                    case Constants.Switch:
                        multiplier = conf.OtherSwitchMultiplier;
                        break;
                    case Constants.ZigZag:

                        double baseValue;
                        if (ranked)
                        {
                            baseValue = ConfigReader.GetConfig().ZigZagBaseMultiplier;
                        }
                        else
                        {
                            baseValue = ConfigReader.GetUnrankedConfig().ZigZagBaseMultiplier;

                        }

                        // Zig zags are special as they can have many notes in them.
                        double zigZagMultiplier = PatternMultiplier.ZigZagMultiplier(segment.NotesPerSecond, ranked);
                        double zigZagLengthMultiplier = PatternMultiplier.ZigZagLengthMultiplier(zigZagChainNoteCount, segment.NotesPerSecond, ranked);
                        multiplier = baseValue + ((zigZagMultiplier - baseValue) * zigZagLengthMultiplier);
                        // Console.WriteLine($"({ranked}) ZIG ZAG | NPS: {segment.NotesPerSecond} | NC: {zigZagChainNoteCount} | Multi: {zigZagMultiplier} | Length: {zigZagLengthMultiplier} | Final: {multiplier}");


                        break;
                    case Constants.TwoStack:
                        multiplier = PatternMultiplier.TwoStackMultiplier(segment.NotesPerSecond, ranked);
                        break;
                    case Constants.ThreeStack:
                        multiplier = PatternMultiplier.ThreeStackMultiplier(segment.NotesPerSecond, ranked);
                        break;
                    case Constants.FourStack:
                        multiplier = PatternMultiplier.FourStackMultiplier(segment.NotesPerSecond, ranked);
                        break;
                    case Constants.SingleStreams:
                        multiplier = PatternMultiplier.StreamMultiplier(segment.NotesPerSecond, ranked);
                        break;
                    case Constants.ShortInterval:
                        multiplier = conf.OtherShortIntMultiplier;
                        break;
                    case Constants.MedInterval:
                        multiplier = conf.OtherMedIntMultiplier;
                        break;
                    case Constants.LongInterval:
                        multiplier = conf.OtherLongIntMultiplier;
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
