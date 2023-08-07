using System;
using System.Linq;

namespace MuseMapalyzr
{
    public class SkewedCirclesCheckSegment : CheckSegmentStrategy
    {
        public SkewedCirclesCheckSegment(Pattern pattern) : base(pattern) { }

        public override bool? CheckSegment(Segment? currentSegment)
        {
            if (!Pattern.IsActive)
                return false;

            Segment previousSegment = Pattern.Segments.Any() ? Pattern.Segments.Last() : null;

            if (Pattern.SegmentIsInterval(currentSegment))
            {
                bool atStart = Pattern.AddIntervalIsAtStart(currentSegment);
                if (!atStart)
                    return false;
                return true;
            }

            if (!Pattern.IsNStack(currentSegment) && currentSegment.SegmentName != Constants.ZigZag)
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

            if (currentSegment.SegmentName == Constants.ZigZag && currentSegment.Notes.Count != 3)
                return false;

            Pattern.Segments.Add(currentSegment);

            return true;
        }
    }

    public class SkewedCirclesIsAppendable : IsAppendableStrategy
    {
        public SkewedCirclesIsAppendable(Pattern pattern) : base(pattern) { }

        public override bool IsAppendable()
        {
            if (Pattern.Segments.Count >= 3)
            {
                int nStackCount = 0;
                foreach (var p in Pattern.Segments)
                {
                    if (Pattern.IsNStack(p))
                        nStackCount += 1;
                    if (!Pattern.IsNStack(p) && p.SegmentName != Constants.ZigZag && !Pattern.SegmentIsInterval(p))
                        throw new Exception($"Skewed Circle has a: {p.SegmentName}!!");
                }
                if (nStackCount >= 2)
                    return true;
            }
            return false;
        }
    }

    public class SkewedCirclesCalcVariationScore : DefaultCalcVariationScore
    {
        public SkewedCirclesCalcVariationScore(Pattern pattern) : base(pattern) { }

        public override double CalcVariationScore()
        {
            double variationScore = base.CalcVariationScore();
            double modifiedVariationScore = Math.Max(1, variationScore);
            return modifiedVariationScore;
        }
    }

    public class SkewedCirclesCalcPatternMultiplier : CalcPatternMultiplierStrategy
    {
        public SkewedCirclesCalcPatternMultiplier(Pattern pattern) : base(pattern) { }

        public override double CalcPatternMultiplier()
        {
            double nps = Pattern.Segments.First().NotesPerSecond;
            double multiplier = PatternMultiplier.SkewedCircleMultiplier(nps); // replace with actual C# equivalent
            Console.WriteLine($"SkewedCirclesCalcPatternMultiplier: {multiplier}");
            return multiplier;
        }
    }
}
