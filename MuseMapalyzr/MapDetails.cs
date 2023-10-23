using System.Collections;
using System.Diagnostics;
using System.Text;
namespace MuseMapalyzr
{
    public class MapDetails
    {

        public List<Note> Notes;
        public int SampleRate;
        public List<double> RankedDensities = new List<double>();
        public List<double> PeakDensities = new List<double>();
        public DensityDetails RankedDensityDetails = new DensityDetails();
        public DensityDetails PeakDensityDetails = new DensityDetails();
        public List<Segment> AnalysedSegments = new List<Segment>();
        public List<Pattern> AnalysedPatterns = new List<Pattern>();
        public List<List<double>> PatternMultiplierSections = new List<List<double>>();
        public double RankedDifficulty = -1;
        public double PeakDifficulty = -1;

        private Dictionary<string, int>? _SimpleSegmentData = null;
        public Dictionary<string, int> SimpleSegmentData
        {
            get
            {
                if (_SimpleSegmentData != null) return _SimpleSegmentData;

                _SimpleSegmentData = GetSimpleSegmentData();
                return _SimpleSegmentData;
            }
        }
        private Dictionary<string, int>? _SimplePatternData = null;

        public Dictionary<string, int> SimplePatternData
        {
            get
            {
                if (_SimplePatternData != null) return _SimplePatternData;

                _SimplePatternData = GetSimplePatternData();
                return _SimplePatternData;
            }
        }

        public MapDetails(
            List<Note> notes,
            int sampleRate
            )
        {
            Notes = notes;
            SampleRate = sampleRate;
        }

        private Dictionary<string, int> GetSimpleSegmentData()
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            foreach (string segmentName in Constants.SegmentTypes)
            {
                results[segmentName] = 0;
            }
            foreach (Segment segment in AnalysedSegments)
            {
                results[segment.SegmentName]++;
            }

            return results;
        }

        public Dictionary<string, int> GetSimplePatternData()
        {
            Dictionary<string, int> results = new Dictionary<string, int>();
            foreach (string patternName in Constants.PatternTypes)
            {
                results[patternName] = 0;
            }

            foreach (Pattern pattern in AnalysedPatterns)
            {
                results[pattern.PatternName]++;
            }

            return results;
        }


        public void CalculateRankedAndPeakValues(PatternWeightingResults patternWeightingResults)
        {
            // Do it for ranked
            double rankedDifficulty = _CalculateDensityAverage(
                new List<double>(RankedDensities),
                ConfigReader.GetConfig().HardestSeconds,
                ConfigReader.GetConfig().Arbitrary90PercentThreshold,
                ConfigReader.GetConfig().RankedPenaltyProportion,
                ConfigReader.GetConfig().CeilingProportion,
                patternWeightingResults.RankedPatternWeightingSections,
                true
                ) * Constants.BaseDifficultyMultiplier;


            // Do it for peak

            double peakDifficulty = _CalculateDensityAverage(
                new List<double>(PeakDensities),
                ConfigReader.GetUnrankedConfig().HardestSeconds,
                ConfigReader.GetUnrankedConfig().Arbitrary90PercentThreshold,
                ConfigReader.GetUnrankedConfig().RankedPenaltyProportion,
                ConfigReader.GetUnrankedConfig().CeilingProportion,
                patternWeightingResults.UnrankedPatternWeightingSections,
                false
                ) * Constants.BaseDifficultyMultiplier;

            double scaledPeakDifficulty = Utils.ScaleDifficulty(peakDifficulty);

            RankedDifficulty = rankedDifficulty;
            PeakDifficulty = scaledPeakDifficulty;

        }

        private double _CalculateDensityAverage(
            List<double> values,
            int hardestSeconds,
            double arbitrary90PercentThreshold,
            double rankedPenaltyProportion,
            double ceilingProportion,
            List<List<double>> patternMultiplierSections,
            bool isRanked
            )
        {

            // Values should be the densities * pattern multipliers in each 1 second windows

            // Check if the list is empty
            if (values.Count == 0 || values == null)
            {
                throw new ArgumentException("Input list is empty.");
            }

            int numTopValues = hardestSeconds;
            List<double> patternMultiplierAverages = new List<double>();
            int index = 0;
            foreach (List<double> patternMultipliers in patternMultiplierSections)
            {
                if (patternMultipliers.Count == 0)
                {
                    patternMultipliers.Add(1);
                }
                // double averagePatternMultiplier = patternMultipliers.Average();
                double averagePatternMultiplier = Utils.WeightedAverageOfValues(patternMultipliers, 0.2, 0.9, 0.1);
                double before = values[index];
                values[index] = values[index] * averagePatternMultiplier;

                patternMultiplierAverages.Add(averagePatternMultiplier);
                string multipliersString = string.Join(", ", patternMultipliers);
                // Console.WriteLine($"Index: {index} [{multipliersString}] | Before: {before} After: {values[index]} {averagePatternMultiplier}x");
                index++;

            }



            // Sort the list in descending order
            List<double> sortedValues = values.OrderByDescending(d => d).ToList();


            List<double> topValues = sortedValues.Take(numTopValues).ToList();
            double hardest = Utils.WeightedAverageOfValues(topValues, 0.2, 0.9, 0.1);

            double additionalStars = ceilingProportion * hardest;

            double ceiling = hardest + additionalStars;



            double cumulativeSumOfDensities = Utils.CalculateWeightedSum(sortedValues, hardest);

            double penalty = hardest * rankedPenaltyProportion; // 0 if calculating peak difficulty
            double finalPenalisedBase = hardest - penalty; // Is just hardest if calculating peak difficulty

            double X = ceiling - finalPenalisedBase; // The number to approach
            double N = arbitrary90PercentThreshold;  // The point where the function should be 90% of X
            double addedDifficulty = Utils.LogarithmicGrowth(cumulativeSumOfDensities, X, N);



            double finalDifficulty = finalPenalisedBase + addedDifficulty;

            DensityDetails densityDetails = new DensityDetails
            {
                Ceiling = ceiling,
                Hardest = hardest,
                HardestDensities = topValues,
                AdditionalStars = additionalStars,
                Penalty = penalty,
                FinalPenalisedBase = finalPenalisedBase,
                CumulativeSum = cumulativeSumOfDensities,
                AddedDifficulty = addedDifficulty,
                PatternAveragedMultipliers = patternMultiplierAverages,
                DensitiesAfterPatternMultiplication = values
            };

            if (isRanked)
            {
                RankedDensityDetails = densityDetails;

            }
            else
            {
                PeakDensityDetails = densityDetails;
            }

            return finalDifficulty;
        }




    }
}