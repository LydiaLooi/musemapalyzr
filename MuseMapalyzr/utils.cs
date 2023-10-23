namespace MuseMapalyzr
{
    public class Utils
    {

        public static double WeightedAverageOfValues(List<double> values, double topPercentage, double topWeight, double bottomWeight, int? numMax = null)
        {
            // Check if the list is empty
            if (values.Count == 0 || values == null)
            {
                throw new ArgumentException("Input list is empty.");
            }

            int numTopValues;
            if (numMax != null)
            {
                // Calculate the number of top values to consider based on the percentage
                numTopValues = Math.Min((int)(values.Count * topPercentage), (int)numMax);
            }
            else
            {
                numTopValues = (int)(values.Count * topPercentage);
            }


            // Sort the list in descending order
            values.Sort((a, b) => -a.CompareTo(b));

            //Console.WriteLine(string.Join(",", values));


            // Calculate the sum of top values with the topWeight
            double topSum = values.Take(numTopValues).Sum() * topWeight;

            // Calculate the sum of the remaining values with the bottomWeight
            double bottomSum = values.Skip(numTopValues).Sum() * bottomWeight;

            // Calculate the final weighted average
            double totalWeight = (numTopValues * topWeight) + ((values.Count - numTopValues) * bottomWeight);
            double weightedAverage = (topSum + bottomSum) / totalWeight;

            //Console.WriteLine($"{weightedAverage} | Top {topPercentage * 100}% Index: {numTopValues} ... Threshold {values[numTopValues]}\n");


            return weightedAverage;
        }

        public static double LogarithmicGrowth(double x, double X, double N)
        {
            double b = (Math.Exp(0.9) - 1) / N;
            return X * Math.Log(b * x + 1);
        }

        public static double CalculateWeightedSum(IEnumerable<double> bottomNums, double hardest)
        {
            double weightedSum = 0;

            double strength = 2.5; // yeah

            foreach (double num in bottomNums)
            {
                double weight = strength / Math.Max(Math.Abs(hardest - num) + 1, 1); // Math.Max 1 to avoid div by 0... 
                weightedSum += num * weight;
            }

            return weightedSum;
        }

        public static double ScaleDifficulty(double originalDifficulty)
        {
            // Target value
            double target = 6.0;
            double scalingFactor = 2;

            // If the original difficulty is less than or equal to the target, return it as is
            if (originalDifficulty <= target)
            {
                return originalDifficulty;
            }

            // Apply a logarithmic transformation to compress values greater than the target
            // The "+ 1" ensures that the value is always greater than the target
            // The "Math.Log(originalDifficulty - target + 1)" compresses the values
            double adjustedDifficulty = target + Math.Log(originalDifficulty - target + 1) * scalingFactor;

            return adjustedDifficulty;
        }

        public static Segment? FindSegmentFromStartNote(Note note, List<Segment> segments)
        {
            foreach (Segment seg in segments)
            {
                if (seg.Notes.First().SampleTime == note.SampleTime)
                {
                    return seg;
                }
            }
            return null;
        }

        public static double GetTimeDifferenceWithNPS(double NPS, double sampleRate)
        {
            return sampleRate / NPS;
        }

    }
}