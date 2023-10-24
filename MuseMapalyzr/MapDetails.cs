using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
namespace MuseMapalyzr
{
    public class MapDetails
    {

        public class SegmentData
        {
            public string SegmentName;
            public int Count = 0;
            public int TotalNotes = 0;
            public double Proportion
            {
                get
                {
                    if (MapNoteCount == 0) return -1;
                    double result = (double)TotalNotes / (double)MapNoteCount;
                    return result;
                }
            }
            public double MinNPS
            {
                get
                {
                    if (NPSs.Count() == 0) return 0;

                    return NPSs.Min();
                }
            }
            public double MaxNPS
            {
                get
                {
                    if (NPSs.Count() == 0) return 0;

                    return NPSs.Max();
                }
            }
            public double AvgNPS
            {
                get
                {
                    if (NPSs.Count() == 0) return 0;
                    return NPSs.Average();
                }
            }
            public List<double> NPSs = new List<double>();

            private int MapNoteCount;

            public SegmentData(string name, int mapNoteCount)
            {
                SegmentName = name;
                MapNoteCount = mapNoteCount;
            }
        }

        public class PatternData
        {
            public string PatternName;
            public int Count = 0;
            public int TotalNotes = 0;
            public int ProportionOfMap
            {
                get
                {
                    if (TotalMapNotes == 0) return -1;
                    return TotalNotes / TotalMapNotes;
                }
            }


            private int TotalMapNotes;

            public Dictionary<string, SegmentData> SegmentsInPattern = new Dictionary<string, SegmentData>();

            public PatternData(string name, int totalMapNotes)
            {
                PatternName = name;
                TotalMapNotes = totalMapNotes;
            }
        }

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
        public double Length = -1;

        private Dictionary<string, SegmentData>? _MapSegmentData = null;
        public Dictionary<string, SegmentData> MapSegmentData
        {
            get
            {
                if (_MapSegmentData != null) return _MapSegmentData;
                _MapSegmentData = GetMapSegmentData();
                return _MapSegmentData;
            }
        }

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

            Length = Utils.GetSongLengthFromNotes(Notes, SampleRate);

        }

        private Dictionary<string, SegmentData> GetMapSegmentData()
        {
            Dictionary<string, SegmentData> results = new Dictionary<string, SegmentData>();
            foreach (string segmentName in Constants.SegmentTypes)
            {
                SegmentData data = new SegmentData(segmentName, Notes.Count());
                results[segmentName] = data;
            }


            foreach (Segment segment in AnalysedSegments)
            {
                results[segment.SegmentName].Count++;

                results[segment.SegmentName].TotalNotes += segment.Notes.Count() - 1; // due to notes overlapping... this is kinda an estimate lol

                results[segment.SegmentName].NPSs.Add(segment.NotesPerSecond);
            }


            return results;
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



            double difficulty = finalPenalisedBase + addedDifficulty;
            double finalDifficulty;
            if (isRanked)
            {
                finalDifficulty = ScaleBasedOnLengthOfSong(difficulty, Length);
            }
            else
            {
                finalDifficulty = difficulty;
            }

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
                DensitiesAfterPatternMultiplication = values,
                BeforeLengthScale = difficulty
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

        private static double ScaleBasedOnLengthOfSong(double difficulty, double length)
        {
            // If difficulty is greater than 90, return it as is
            double threshold = ConfigReader.GetConfig().NormalSizedMapThreshold;
            if (difficulty > threshold)
            {
                return difficulty;
            }
            // Calculate the difference between 90 and the difficulty
            double difference = threshold - length;
            double minScaling = 0.5;
            double gradient = 0.005495; // So that at x = 91, y = 0.5
            double scaling = Math.Min(1, Math.Max(1 - gradient * difference, minScaling));

            double scaledDifficulty = difficulty * scaling;


            // Return the scaled difficulty
            return scaledDifficulty;




        }
    }
}