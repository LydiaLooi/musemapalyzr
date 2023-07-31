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
        public override Pattern CreateCopy()
        {
            return new EvenCirclesGroup(PatternName, Segments, StartSample, EndSample, SampleRate);
        }

    }


    public class SkewedCirclesGroup : Pattern
    {
        public SkewedCirclesGroup(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new SkewedCirclesCheckSegment(this));
            SetIsAppendableStrategy(new SkewedCirclesIsAppendable(this));
            SetCalcVariationScoreStrategy(new SkewedCirclesCalcVariationScore(this));
            SetCalcPatternMultiplierStrategy(new SkewedCirclesCalcPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }
        public override Pattern CreateCopy()
        {
            return new SkewedCirclesGroup(PatternName, Segments, StartSample, EndSample, SampleRate);
        }
    }

    public class NothingButTheoryGroup : Pattern
    {
        public NothingButTheoryGroup(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new NothingButTheoryCheckSegment(this));
            SetIsAppendableStrategy(new NothingButTheoryIsAppendable(this));
            SetCalcVariationScoreStrategy(new NothingButTheoryCalcVariationScore(this));
            SetCalcPatternMultiplierStrategy(new NothingButTheoryCalcPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }
        public override Pattern CreateCopy()
        {
            return new NothingButTheoryGroup(PatternName, Segments, StartSample, EndSample, SampleRate);
        }
    }

    public class SlowStretchPattern : Pattern
    {
        public SlowStretchPattern(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new SlowStretchCheckSegment(this));
            SetIsAppendableStrategy(new SlowStretchIsAppendable(this));
            SetCalcVariationScoreStrategy(new SlowStretchCalcVariation(this));
            SetCalcPatternMultiplierStrategy(new SlowStretchPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }
        public override Pattern CreateCopy()
        {
            return new SlowStretchPattern(PatternName, Segments, StartSample, EndSample, SampleRate);
        }
    }

    public class VaryingStacksPattern : Pattern
    {
        public VaryingStacksPattern(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new VaryingStacksCheckSegment(this));
            SetIsAppendableStrategy(new VaryingStacksIsAppendable(this));
            SetCalcVariationScoreStrategy(new VaryingStacksCalcVariationScore(this));
            SetCalcPatternMultiplierStrategy(new VaryingStacksCalcPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }
        public override Pattern CreateCopy()
        {
            return new VaryingStacksPattern(PatternName, Segments, StartSample, EndSample, SampleRate);
        }
    }

    public class OtherPattern : Pattern
    {
        public OtherPattern(string patternName, List<Segment> segments, int startSample = 0, int endSample = 0, int sampleRate = 0)
            : base(patternName, segments, startSample, endSample, sampleRate)
        {
            SetCheckSegmentStrategy(new OtherCheckSegment(this));
            SetIsAppendableStrategy(new OtherIsAppendable(this));
            SetCalcVariationScoreStrategy(new OtherCalcVariationScore(this));
            SetCalcPatternMultiplierStrategy(new OtherCalcPatternMultiplier(this));
            SetCalcPatternLengthMultiplierStrategy(new DefaultCalcPatternLengthMultiplier(this));
        }
        public override Pattern CreateCopy()
        {
            return new OtherPattern(PatternName, Segments, StartSample, EndSample, SampleRate);
        }
    }

}