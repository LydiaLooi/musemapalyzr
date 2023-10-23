using System.Collections;
using System.Diagnostics;
using System.Text;
namespace MuseMapalyzr
{
    public class SectionResults
    {
        public List<List<Note>> RankedSections;
        public List<List<Note>> UnrankedSections;

        public SectionResults(List<List<Note>> rankedSections, List<List<Note>> unrankedSections)
        {
            RankedSections = rankedSections;
            UnrankedSections = unrankedSections;
        }
    }

    public class DensityDetails
    {
        public double Ceiling;
        public double Hardest;
        public List<double> HardestDensities = new List<double>();
        public List<double> PatternAveragedMultipliers = new List<double>();
        public List<double> DensitiesAfterPatternMultiplication = new List<double>();
        public double Penalty;
        public double FinalPenalisedBase;
        public double AdditionalStars;
        public double CumulativeSum;
        public double AddedDifficulty;
        public double BeforeLengthScale;

    }


    public class WeightingResults
    {
        public double RankedWeighting;
        public double RankedDifficulty;
        public double RankedWeightedDifficulty;

        public double UnrankedWeighting;
        public double UnrankedDifficulty;
        public double UnrankedWeightedDifficulty;

        public WeightingResults(
            double rankedWeighting,
            double rankedDifficulty,
            double rankedWeightedDifficulty,
            double unrankedWeighting,
            double unrankedDifficulty,
            double unrankedWeightedDifficulty
            )
        {
            RankedWeighting = rankedWeighting;
            RankedDifficulty = rankedDifficulty;
            RankedWeightedDifficulty = rankedWeightedDifficulty;

            UnrankedWeighting = unrankedWeighting;
            UnrankedDifficulty = unrankedDifficulty;
            UnrankedWeightedDifficulty = unrankedWeightedDifficulty;

        }
    }


    public class SegmentMultipliers
    {
        public List<double> PeakMultipliers;
        public List<double> RankedMultipliers;
        public SegmentMultipliers()
        {
            PeakMultipliers = new List<double>();
            RankedMultipliers = new List<double>();
        }
    }

    public class PatternWeightingResults
    {
        public double RankedPatternWeighting;
        public double UnrankedPatternWeighting;
        public List<Pattern> IdentifiedPatterns = new List<Pattern>();
        public List<Segment> IdentifiedSegments = new List<Segment>();
        public SegmentMultipliers SegmentMultipliers = new SegmentMultipliers();

        public List<List<double>> RankedPatternWeightingSections = new List<List<double>>();
        public List<List<double>> UnrankedPatternWeightingSections = new List<List<double>>();

        public List<Segment>? _StreamSegments;
        public List<Segment> StreamSegments
        {
            get
            {
                if (_StreamSegments == null)
                {
                    List<Segment> temp = new List<Segment>();
                    foreach (Segment segment in IdentifiedSegments)
                    {
                        if (
                            segment.SegmentName == Constants.SingleStreams
                            || segment.SegmentName == Constants.FourStack
                            || segment.SegmentName == Constants.ThreeStack
                            || segment.SegmentName == Constants.TwoStack
                            )
                        {
                            temp.Add(segment);
                        }
                    }
                    _StreamSegments = temp;
                }
                return _StreamSegments;
            }
        }
    }

}