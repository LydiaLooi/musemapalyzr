namespace MuseMapalyzr
{
    public static class Constants
    {
        public static string DATA_DIR = "data";
        public static string OUTPUT_DIR = "difficulty_exports";
        public static string MAP_OUTPUTS = "map_outputs";
        public static int DEFAULT_SAMPLE_RATE = 44100; // time_s * TIME_CONVERSION = sample_time

        public const string SWITCH = "Switch";
        public const string ZIG_ZAG = "Zig Zag";
        public const string TWO_STACK = "2-Stack";
        public const string THREE_STACK = "3-Stack";
        public const string FOUR_STACK = "4-Stack";
        public const string SINGLE_STREAMS = "Single Streams";

        public const string SHORT_INTERVAL = "Short Interval";
        public const string MED_INTERVAL = "Medium Interval";
        public const string LONG_INTERVAL = "Long Interval";

        public const string SLOW_STRETCH = "Slow Stretch";
        public const string EVEN_CIRCLES = "Even Circles";
        public const string SKEWED_CIRCLES = "Skewed Circles";
        public const string VARYING_STACKS = "Varying Stacks";
        public const string NOTHING_BUT_THEORY = "Nothing But Theory";
        public const string VARIABLE_STREAM = "Variable Stream";
        public const string OTHER = "Other";

        public static int GetTolerance()
        {
            dynamic config = ConfigReader.GetConfig();
            int tolerance = config["pattern_tolerance_ms"] * DEFAULT_SAMPLE_RATE / 1000;
            return tolerance;
        }

    }
}