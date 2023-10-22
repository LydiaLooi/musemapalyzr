using System;
using System.Collections;
using static MuseMapalyzr.ConfigReader;

namespace MuseMapalyzr
{


    public class PatternMultiplier
    {
        public enum ZigZagType
        {
            Sustainable,
            Unsustainable
        }

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


        public static double ZigZagMultiplier(double nps, bool ranked)
        {
            double maxSusMultiplier = GetRightConfig(ranked).ZigZagUpBound;

            double zigZagBaseMultiplier = GetRightConfig(ranked).ZigZagBaseMultiplier;

            double unsusThresholdNPS = GetRightConfig(ranked).UnsusZigZagThresholdNPS;

            double minUnsusMultiplier = GetRightConfig(ranked).ZigZagUnsusStartingMultiplier;
            double susBeginNPS = GetRightConfig(ranked).ZigZagSusBeginningNPS;

            double unsusGradient = GetRightConfig(ranked).ZigZagUnsusGradient;

            double lowerClamp = GetRightConfig(ranked).ZigZagLowClamp;
            double upperClamp = GetRightConfig(ranked).ZigZagUpClamp;

            double lowerBound = GetRightConfig(ranked).ZigZagLowBound;
            double upperBound = maxSusMultiplier;

            // Initialize result variable
            double result;


            // Case 1: nps is less than b
            if (nps < susBeginNPS)
            {
                return zigZagBaseMultiplier;
            }
            else if (nps >= susBeginNPS && nps < unsusThresholdNPS)
            {

                // Cubic function for ease-in-out curve
                double t = (nps - susBeginNPS - lowerClamp) / (unsusThresholdNPS - susBeginNPS - upperClamp);
                t = Clamp(t, 0, 1);

                return lowerBound + (upperBound - lowerBound) * SmoothStep(t);
            }
            // Case 2: nps is greater than or equal to b
            else
            {
                // Line with gradient, starting at (b, c)
                result = unsusGradient * (nps - unsusThresholdNPS) + minUnsusMultiplier;
            }

            // Return the result
            return result;

        }



        public static double ZigZagLengthMultiplier(double numNotes, double nps, bool ranked)
        {


            double susLowerClamp = GetRightConfig(ranked).SusZigZagLengthLowClamp;
            double susUpperClamp = GetRightConfig(ranked).SusZigZagLengthUpClamp;
            double susLowerBound = GetRightConfig(ranked).SusZigZagLengthLowBound;
            double susUpperBound = GetRightConfig(ranked).SusZigZagLengthUpBound;

            double unsusLowerClamp = GetRightConfig(ranked).UnsusZigZagLengthLowClamp;
            double unsusUpperClamp = GetRightConfig(ranked).UnsusZigZagLengthUpClamp;
            double unsusLowerBound = GetRightConfig(ranked).UnsusZigZagLengthLowBound;
            double unsusUpperBound = GetRightConfig(ranked).UnsusZigZagLengthUpBound;


            double unsusThresholdNPS = GetRightConfig(ranked).UnsusZigZagThresholdNPS;



            ZigZagType zigZagType;
            if (!ranked)
            {
                zigZagType = ZigZagType.Sustainable;
            }
            else if (ranked && nps < unsusThresholdNPS)
            {
                zigZagType = ZigZagType.Sustainable;
            }
            else
            {
                zigZagType = ZigZagType.Unsustainable;
            }

            // Console.WriteLine(zigZagType.ToString());

            if (zigZagType == ZigZagType.Sustainable)
            {
                // Cubic function for ease-in-out curve
                double t = (numNotes - susLowerClamp) / (susUpperClamp - susLowerClamp);
                t = Clamp(t, 0, 1);

                return susLowerBound + (susUpperBound - susLowerBound) * SmoothStep(t);
            }
            else
            {
                // Cubic function for ease-in-out curve
                double t = (numNotes - unsusLowerClamp) / (unsusUpperClamp - unsusLowerClamp);
                t = Clamp(t, 0, 1);
                t = 1 - t; // Flip the curve horizontally by replacing t with 1 - t


                return unsusLowerBound + (unsusUpperBound - unsusLowerBound) * SmoothStep(t);
            }



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
    }
}