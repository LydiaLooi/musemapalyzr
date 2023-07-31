namespace MuseMapalyzr
{
    public static class Constants
    {
        public static string DataDir = "data";
        public static string OutputDir = "difficulty_exports";
        public static string MapOutputs = "map_outputs";
        public static string AnalysisDir = "analysis";
        public static int DefaultSampleRate = 44100; // time_s * TIME_CONVERSION = sample_time

        public const string Switch = "Switch";
        public const string ZigZag = "Zig Zag";
        public const string TwoStack = "2-Stack";
        public const string ThreeStack = "3-Stack";
        public const string FourStack = "4-Stack";
        public const string SingleStreams = "Single Streams";

        public const string ShortInterval = "Short Interval";
        public const string MedInterval = "Medium Interval";
        public const string LongInterval = "Long Interval";

        public const string SlowStretch = "Slow Stretch";
        public const string EvenCircles = "Even Circles";
        public const string SkewedCircles = "Skewed Circles";
        public const string VaryingStacks = "Varying Stacks";
        public const string NothingButTheory = "Nothing But Theory";
        public const string VariableStream = "Variable Stream";
        public const string Other = "Other";

        public static int GetTolerance()
        {
            dynamic config = ConfigReader.GetConfig();
            int tolerance = config["pattern_tolerance_ms"] * DefaultSampleRate / 1000;
            return tolerance;
        }

    }
}