namespace MuseMapalyzr
{
    public class PatternMultiplier
    {
        private static dynamic conf = ConfigReader.GetConfig();

        private static double SmoothStep(double x)
        {
            return x * x * (3 - 2 * x);
        }

        private static double EaseInCubic(double x)
        {
            return Math.Pow(x, 4);
        }

        private static double EaseInOut(double x)
        {
            return 3 * Math.Pow(x, 2) - 2 * Math.Pow(x, 3);
        }

        private static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static double NothingButTheoryMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["nothing_but_theory_low_bound"]);
            double upperBound = double.Parse(conf["nothing_but_theory_up_bound"]);
            double lowerClamp = double.Parse(conf["nothing_but_theory_low_clamp"]);
            double upperClamp = double.Parse(conf["nothing_but_theory_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStreams(double nps)
        {
            double lowerBound = double.Parse(conf["varying_streams_low_bound"]);
            double upperBound = double.Parse(conf["varying_streams_up_bound"]);
            double lowerClamp = double.Parse(conf["varying_streams_low_clamp"]);
            double upperClamp = double.Parse(conf["varying_streams_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["zig_zag_low_bound"]);
            double upperBound = double.Parse(conf["zig_zag_up_bound"]);
            double lowerClamp = double.Parse(conf["zig_zag_low_clamp"]);
            double upperClamp = double.Parse(conf["zig_zag_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);
        }

        public static double EvenCircleMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["even_circle_low_bound"]);
            double upperBound = double.Parse(conf["even_circle_up_bound"]);

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double SkewedCircleMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["skewed_circle_low_bound"]);
            double upperBound = double.Parse(conf["skewed_circle_up_bound"]);

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double StreamMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["stream_low_bound"]);
            double upperBound = double.Parse(conf["stream_up_bound"]);
            double lowerClamp = double.Parse(conf["stream_low_clamp"]);
            double upperClamp = double.Parse(conf["stream_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double PatternStreamLengthMultiplier(double numNotes)
        {
            double lowerBound = double.Parse(conf["pattern_stream_length_low_bound"]);
            double upperBound = double.Parse(conf["pattern_stream_length_up_bound"]);
            double lowerClamp = double.Parse(conf["pattern_stream_length_low_clamp"]);
            double upperClamp = double.Parse(conf["pattern_stream_length_up_clamp"]);

            double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagLengthMultiplier(double numNotes, double nps)
        {
            double lowerBound = double.Parse(conf["zig_zag_length_low_bound"]);
            double upperBound = double.Parse(conf["zig_zag_length_up_bound"]);
            double lowerClamp = double.Parse(conf["zig_zag_length_low_clamp"]);
            double upperClamp = double.Parse(conf["zig_zag_length_up_clamp"]);
            double npsThreshold = double.Parse(conf["zig_zag_length_nps_threshold"]);
            // Console.WriteLine($"NPS: {nps}, NOTES: {numNotes}");
            if (nps > npsThreshold)
            {
                double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
                t = Clamp(t, 0, 1);
                return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);

            }
            return 1;
        }

        public static double FourStackMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["four_stack_low_bound"]);
            double upperBound = double.Parse(conf["four_stack_up_bound"]);
            double lowerClamp = double.Parse(conf["four_stack_low_clamp"]);
            double upperClamp = double.Parse(conf["four_stack_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }


        public static double ThreeStackMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["three_stack_low_bound"]);
            double upperBound = double.Parse(conf["three_stack_up_bound"]);
            double lowerClamp = double.Parse(conf["three_stack_low_clamp"]);
            double upperClamp = double.Parse(conf["three_stack_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double TwoStackMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["two_stack_low_bound"]);
            double upperBound = double.Parse(conf["two_stack_up_bound"]);
            double lowerClamp = double.Parse(conf["two_stack_low_clamp"]);
            double upperClamp = double.Parse(conf["two_stack_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStacksMultiplier(double nps)
        {
            double lowerBound = double.Parse(conf["varying_stacks_low_bound"]);
            double upperBound = double.Parse(conf["varying_stacks_up_bound"]);
            double lowerClamp = double.Parse(conf["varying_stacks_low_clamp"]);
            double upperClamp = double.Parse(conf["varying_stacks_up_clamp"]);

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }
    }
}