namespace MuseMapalyzr
{
    public class EvenCirclesGroup : Pattern
    {
        public EvenCirclesGroup(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new EvenCirclesCheckSegment(this));
            SetIsAppendableStrategy(new EvenCirclesIsAppendable(this));
            SetCalcVariationScoreStrategy(new EvenCirclesCalcVariationScore(this));
            SetCalcPatternMultiplierStrategy(new EvenCirclesCalcPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }

        public void SetCheckSegmentStrategy(object strategy)
        {
            CheckSegmentStrategy = strategy;
        }

        public void SetIsAppendableStrategy(object strategy)
        {
            IsAppendableStrategy = strategy;
        }

        public void SetCalcVariationScoreStrategy(object strategy)
        {
            CalcVariationScoreStrategy = strategy;
        }

        public void SetCalcPatternMultiplierStrategy(object strategy)
        {
            CalcPatternMultiplierStrategy = strategy;
        }

        public void SetCalcPatternLengthMultiplierStrategy(object strategy)
        {
            CalcPatternLengthMultiplierStrategy = strategy;
        }
    }
}