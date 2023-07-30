namespace MuseMapalyzr
{

    public abstract class CheckSegmentStrategy
    {
        protected Pattern Pattern;

        public CheckSegmentStrategy(Pattern pattern)
        {
            Pattern = pattern;
        }

        public abstract bool? CheckSegment(Segment currentSegment);
    }

    public abstract class IsAppendableStrategy
    {
        protected Pattern Pattern;

        public IsAppendableStrategy(Pattern pattern)
        {
            Pattern = pattern;
        }

        public abstract bool IsAppendable();
    }

    public abstract class CalcVariationScoreStrategy
    {
        protected Pattern Pattern;

        public CalcVariationScoreStrategy(Pattern pattern)
        {
            Pattern = pattern;
        }

        public abstract float CalcVariationScore();
    }

    public abstract class CalcPatternMultiplierStrategy
    {
        protected Pattern Pattern;

        public CalcPatternMultiplierStrategy(Pattern pattern)
        {
            Pattern = pattern;
        }

        public abstract float CalcPatternMultiplier();
    }

    public abstract class CalcPatternLengthMultiplierStrategy
    {
        protected Pattern Pattern;

        public CalcPatternLengthMultiplierStrategy(Pattern pattern)
        {
            Pattern = pattern;
        }

        public abstract float CalcPatternLengthMultiplier();
    }
}