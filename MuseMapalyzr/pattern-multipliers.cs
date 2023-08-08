using System;
using static MuseMapalyzr.ConfigReader;

namespace MuseMapalyzr
{
    public class PatternMultiplier
    {
        private static MuseMapalyzrConfig conf = ConfigReader.GetConfig();

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
            double lowerBound = conf.NothingButTheoryLowBound;
            double upperBound = conf.NothingButTheoryUpBound;
            double lowerClamp = conf.NothingButTheoryLowClamp;
            double upperClamp = conf.NothingButTheoryUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStreams(double nps)
        {
            double lowerBound = conf.VaryingStreamsLowBound;
            double upperBound = conf.VaryingStreamsUpBound;
            double lowerClamp = conf.VaryingStreamsLowClamp;
            double upperClamp = conf.VaryingStreamsUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagMultiplier(double nps)
        {
            double lowerBound = conf.ZigZagLowBound;
            double upperBound = conf.ZigZagUpBound;
            double lowerClamp = conf.ZigZagLowClamp;
            double upperClamp = conf.ZigZagUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);
        }

        public static double EvenCircleMultiplier(double nps)
        {
            double lowerBound = conf.EvenCircleLowBound;
            double upperBound = conf.EvenCircleUpBound;

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double SkewedCircleMultiplier(double nps)
        {
            double lowerBound = conf.SkewedCircleLowBound;
            double upperBound = conf.SkewedCircleUpBound;

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double StreamMultiplier(double nps)
        {
            double lowerBound = conf.StreamLowBound;
            double upperBound = conf.StreamUpBound;
            double lowerClamp = conf.StreamLowClamp;
            double upperClamp = conf.StreamUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double PatternStreamLengthMultiplier(double numNotes)
        {
            double lowerBound = conf.PatternStreamLengthLowBound;
            double upperBound = conf.PatternStreamLengthUpBound;
            double lowerClamp = conf.PatternStreamLengthLowClamp;
            double upperClamp = conf.PatternStreamLengthUpClamp;

            double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagLengthMultiplier(double numNotes, double nps)
        {
            double lowerBound = conf.ZigZagLengthLowBound;
            double upperBound = conf.ZigZagLengthUpBound;
            double lowerClamp = conf.ZigZagLengthLowClamp;
            double upperClamp = conf.ZigZagLengthUpClamp;
            double npsThreshold = conf.ZigZagLengthNpsThreshold;

            if (nps > npsThreshold)
            {
                double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
                t = Clamp(t, 0, 1);
                return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);
            }
            return 1;
        }

        public static double NothingButTheoryLengthMultiplier(double numNotes, double multiplier)
        {
            double lowerBound = 0;
            double upperBound = 1;
            double lowerClamp = 6;
            double upperClamp = 10;

            double additional = multiplier - 1;
            double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);

            double lengthMultiplier = lowerBound + (upperBound - lowerBound) * SmoothStep(t);
            double newAddtional = lengthMultiplier * additional;
            return 1 + newAddtional;
        }

        public static double FourStackMultiplier(double nps)
        {
            double lowerBound = conf.FourStackLowBound;
            double upperBound = conf.FourStackUpBound;
            double lowerClamp = conf.FourStackLowClamp;
            double upperClamp = conf.FourStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }


        public static double ThreeStackMultiplier(double nps)
        {
            double lowerBound = conf.ThreeStackLowBound;
            double upperBound = conf.ThreeStackUpBound;
            double lowerClamp = conf.ThreeStackLowClamp;
            double upperClamp = conf.ThreeStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double TwoStackMultiplier(double nps)
        {
            double lowerBound = conf.TwoStackLowBound;
            double upperBound = conf.TwoStackUpBound;
            double lowerClamp = conf.TwoStackLowClamp;
            double upperClamp = conf.TwoStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStacksMultiplier(double nps)
        {
            double lowerBound = conf.VaryingStacksLowBound;
            double upperBound = conf.VaryingStacksUpBound;
            double lowerClamp = conf.VaryingStacksLowClamp;
            double upperClamp = conf.VaryingStacksUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }


    }
}