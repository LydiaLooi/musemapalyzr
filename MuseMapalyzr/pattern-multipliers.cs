using System;
using static MuseMapalyzr.ConfigReader;

namespace MuseMapalyzr
{
    public class PatternMultiplier
    {
        private static MuseMapalyzrConfig Conf = ConfigReader.GetConfig();
        private static MuseMapalyzrConfig UnrankedConf = ConfigReader.GetUnrankedConfig();

        private static MuseMapalyzrConfig GetRightConfig(bool ranked)
        {
            if (ranked) return Conf;
            return UnrankedConf;
        }

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

        public static double NothingButTheoryMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).NothingButTheoryLowBound;
            double upperBound = GetRightConfig(ranked).NothingButTheoryUpBound;
            double lowerClamp = GetRightConfig(ranked).NothingButTheoryLowClamp;
            double upperClamp = GetRightConfig(ranked).NothingButTheoryUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStreams(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).VaryingStreamsLowBound;
            double upperBound = GetRightConfig(ranked).VaryingStreamsUpBound;
            double lowerClamp = GetRightConfig(ranked).VaryingStreamsLowClamp;
            double upperClamp = GetRightConfig(ranked).VaryingStreamsUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).ZigZagLowBound;
            double upperBound = GetRightConfig(ranked).ZigZagUpBound;
            double lowerClamp = GetRightConfig(ranked).ZigZagLowClamp;
            double upperClamp = GetRightConfig(ranked).ZigZagUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);
        }

        public static double EvenCircleMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).EvenCircleLowBound;
            double upperBound = GetRightConfig(ranked).EvenCircleUpBound;

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double SkewedCircleMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).SkewedCircleLowBound;
            double upperBound = GetRightConfig(ranked).SkewedCircleUpBound;

            double t = Clamp(nps / 30, 0, 1);
            return lowerBound + (upperBound - lowerBound) * EaseInOut(t);
        }

        public static double StreamMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).StreamLowBound;
            double upperBound = GetRightConfig(ranked).StreamUpBound;
            double lowerClamp = GetRightConfig(ranked).StreamLowClamp;
            double upperClamp = GetRightConfig(ranked).StreamUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double PatternStreamLengthMultiplier(double numNotes, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).PatternStreamLengthLowBound;
            double upperBound = GetRightConfig(ranked).PatternStreamLengthUpBound;
            double lowerClamp = GetRightConfig(ranked).PatternStreamLengthLowClamp;
            double upperClamp = GetRightConfig(ranked).PatternStreamLengthUpClamp;

            double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
            t = Clamp(t, 0, 1);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double ZigZagLengthMultiplier(double numNotes, double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).ZigZagLengthLowBound;
            double upperBound = GetRightConfig(ranked).ZigZagLengthUpBound;
            double lowerClamp = GetRightConfig(ranked).ZigZagLengthLowClamp;
            double upperClamp = GetRightConfig(ranked).ZigZagLengthUpClamp;
            double npsThreshold = GetRightConfig(ranked).ZigZagLengthNpsThreshold;

            if (nps > npsThreshold)
            {
                double t = (numNotes - lowerClamp) / (upperClamp - lowerClamp);
                t = Clamp(t, 0, 1);
                return lowerBound + (upperBound - lowerBound) * EaseInCubic(t);
            }
            return 1;
        }

        public static double NothingButTheoryLengthMultiplier(double numNotes, double multiplier, bool ranked)
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

        public static double FourStackMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).FourStackLowBound;
            double upperBound = GetRightConfig(ranked).FourStackUpBound;
            double lowerClamp = GetRightConfig(ranked).FourStackLowClamp;
            double upperClamp = GetRightConfig(ranked).FourStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }


        public static double ThreeStackMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).ThreeStackLowBound;
            double upperBound = GetRightConfig(ranked).ThreeStackUpBound;
            double lowerClamp = GetRightConfig(ranked).ThreeStackLowClamp;
            double upperClamp = GetRightConfig(ranked).ThreeStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double TwoStackMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).TwoStackLowBound;
            double upperBound = GetRightConfig(ranked).TwoStackUpBound;
            double lowerClamp = GetRightConfig(ranked).TwoStackLowClamp;
            double upperClamp = GetRightConfig(ranked).TwoStackUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }

        public static double VaryingStacksMultiplier(double nps, bool ranked)
        {
            double lowerBound = GetRightConfig(ranked).VaryingStacksLowBound;
            double upperBound = GetRightConfig(ranked).VaryingStacksUpBound;
            double lowerClamp = GetRightConfig(ranked).VaryingStacksLowClamp;
            double upperClamp = GetRightConfig(ranked).VaryingStacksUpClamp;

            double t = (nps - lowerClamp) / (upperClamp - lowerClamp);
            t = Math.Max(Math.Min(t, 1), 0);
            return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
        }


    }
}