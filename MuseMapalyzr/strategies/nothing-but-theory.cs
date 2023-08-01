using System;
using System.Linq;

namespace MuseMapalyzr
{
    public class NothingButTheoryCheckSegment : CheckSegmentStrategy
    {
        public NothingButTheoryCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
        {
            if (!Pattern.IsActive)
                return false;

            var previousSegment = Pattern.Segments.Any() ? Pattern.Segments.Last() : null;

            if (Pattern.SegmentIsInterval(currentSegment))
            {
                var atStart = Pattern.AddIntervalIsAtStart(currentSegment);
                if (!atStart)
                    return false;
                return true;
            }

            // Check for invalid combinations of previous segment and current segment
            if (!Pattern.IsNStack(currentSegment) && currentSegment.SegmentName != Constants.ZigZag)
                return false;

            if (currentSegment.SegmentName == Constants.ZigZag && (currentSegment.Notes.Count != 4 && currentSegment.Notes.Count != 6))
                return false;

            if (previousSegment != null)
            {
                if (Pattern.SegmentIsInterval(previousSegment))
                {
                    Pattern.Segments.Add(currentSegment);
                    return true;
                }

                if (previousSegment.SegmentName == Constants.ZigZag && !Pattern.IsNStack(currentSegment))
                    return false;

                if (Pattern.IsNStack(previousSegment) && currentSegment.SegmentName != Constants.ZigZag)
                    return false;

                if (Math.Abs(currentSegment.TimeDifference.Value - previousSegment.TimeDifference.Value) > Pattern.Tolerance)
                    return false;

                if (!Pattern.IntervalBetweenSegmentsIsTolerable(previousSegment, currentSegment))
                    return false;
            }

            // Current segment should be valid from here
            Pattern.Segments.Add(currentSegment);
            return true;
        }
    }

    public class NothingButTheoryIsAppendable : IsAppendableStrategy
    {

        public NothingButTheoryIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            if (Pattern.Segments.Count >= 3)
            {
                int nStackCount = 0;
                foreach (var seg in Pattern.Segments)
                {
                    if (Pattern.IsNStack(seg))
                        nStackCount++;

                    if (!Pattern.IsNStack(seg) && seg.SegmentName != Constants.ZigZag && !Pattern.SegmentIsInterval(seg))
                        throw new Exception($"Nothing but theory has a: {seg.SegmentName}!!");
                }

                if (nStackCount >= 2)
                    return true;
            }

            return false;
        }
    }

    public class NothingButTheoryCalcVariationScore : CalcVariationScoreStrategy
    {
        public NothingButTheoryCalcVariationScore(Pattern pattern) : base(pattern) { }
        public override double CalcVariationScore()
        {
            // Console.WriteLine("Note: Nothing but theory overrode calc_variation_score");

            var tempLst = Pattern.Segments.Select(s => $"{s.SegmentName} {s.Notes.Count}").ToList();
            var intervalList = new List<double>();
            var segmentNames = new List<string>();

            var segmentCounts = Pattern.GetSegmentTypeCounts(Pattern.Segments.Select(s => s.SegmentName).ToList());

            // Check for intervals:
            for (int i = 0; i < tempLst.Count; i++)
            {
                var name = tempLst[i];
                if (Pattern.Intervals.ContainsKey(name))
                {
                    if (i == 0 || i == tempLst.Count - 1)
                    {
                        intervalList.Add(Pattern.Intervals[name] * Pattern.EndExtraDebuff);
                    }
                    else
                    {
                        intervalList.Add(Pattern.Intervals[name]);
                        segmentNames.Add("Interval");
                    }
                }
                else
                {
                    segmentNames.Add(name);
                }
            }

            Console.WriteLine($"Checking entropy of: {string.Join(", ", segmentNames)}");

            int n = segmentNames.Count;
            var freqDict = new Dictionary<string, int>();
            foreach (var name in segmentNames)
            {
                if (!freqDict.ContainsKey(name))
                    freqDict[name] = 0;
                freqDict[name]++;
            }

            var freq = freqDict.Select(x => (double)x.Value / n).ToList();
            double entropy = -freq.Sum(p => p * Math.Log2(Math.Max(p, 1e-10)));

            if (intervalList.Any())
            {
                // average interval debuffs and multiply that by the entropy
                double averageDebuff = intervalList.Average();
                entropy *= averageDebuff;

                // Console.WriteLine($">>> Debuffing (due to Intervals) by {averageDebuff} <<<");
            }

            entropy = Pattern.CalcSwitchDebuff(segmentCounts, entropy);

            return Math.Max(1, entropy);
        }
    }

    public class NothingButTheoryCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public NothingButTheoryCalcPatternMultiplier(Pattern pattern) : base(pattern) { }
        public override double CalcPatternMultiplier()
        {
            double nps = Pattern.Segments[0].NotesPerSecond;
            double multiplier = PatternMultiplier.NothingButTheoryMultiplier(nps);
            return multiplier;
        }
    }
}
