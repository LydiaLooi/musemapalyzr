namespace MuseMapalyzr
{
    public class DifficultyCalculation
    {
        public class WeightingResults // Named Tuple "Weighting" in python code
        {
            public int Weighting;
            public int Difficulty;
            public int WeightedDifficulty;
        }


        /// Creates sections of section_threshold_seconds length to help with calculating
        /// density over the course of a map.
        public List<List<Note>> CreateSections(
            List<Note> notes, int sectionThresholdSeconds = 1, int? sampleRate = null
        )
        {
            if (sampleRate == null) { sampleRate = Constants.DefaultSampleRate; }

            int sectionThreshold = sectionThresholdSeconds * (int)sampleRate;
            int songStartSamples = notes.Min(note => note.SampleTime);
            int songDurationSamples = notes.Max(note => note.SampleTime);

            // Sorting the 'notes' list based on the 'SampleTime' property
            notes = notes.OrderBy(note => note.SampleTime).ToList();

            int numSections = (songDurationSamples - songStartSamples + sectionThreshold) / sectionThreshold;

            List<List<Note>> sections = new List<List<Note>>();

            // Initialize empty sections
            for (int i = 0; i < numSections; i++)
            {
                sections.Add(new List<Note>());
            }

            // Fill sections with notes
            foreach (Note note in notes)
            {
                int sectionIndex = (note.SampleTime - songStartSamples) / sectionThreshold;
                if (0 <= sectionIndex && sectionIndex < sections.Count)
                {
                    sections[sectionIndex].Add(note);
                }
            }

            return sections;

        }

        public static List<double> MovingAverageNoteDensity(List<List<Note>> sections, int windowSize)
        {
            int numSections = sections.Count;
            List<int> noteDensities = new List<int>();
            foreach (var section in sections)
            {
                noteDensities.Add(section.Count);
            }

            List<double> movingAverages = new List<double>();

            for (int i = 0; i < numSections; i++)
            {
                int start = Math.Max(0, i - windowSize + 1);
                int end = i + 1;
                List<int> window = noteDensities.GetRange(start, end - start);
                double average = window.Average();
                movingAverages.Add(average);
            }

            return movingAverages;
        }


        public static double WeightedAverageOfValues(List<double> values, double topPercentage = 0.3, double topWeight = 0.7, double bottomWeight = 0.3)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("The input list 'values' cannot be empty.");
            }

            // Find the threshold that separates the top 30% highest densities from the rest
            List<double> movingAveragesSorted = new List<double>(values);
            movingAveragesSorted.Sort();
            movingAveragesSorted.Reverse();

            int thresholdIndex = (int)(values.Count * (1 - topPercentage));
            if (thresholdIndex >= movingAveragesSorted.Count)
            {
                thresholdIndex = movingAveragesSorted.Count - 1;
            }
            double threshold = movingAveragesSorted[thresholdIndex];

            // Calculate the weighted average
            double totalWeight = 0;
            double weightedSum = 0;

            foreach (double avg in values)
            {
                double weight;
                if (avg >= threshold)
                {
                    weight = topWeight;
                }
                else
                {
                    weight = bottomWeight;
                }

                weightedSum += avg * weight;
                totalWeight += weight;
            }

            double weightedAverage = weightedSum / totalWeight;
            return weightedAverage;
        }

        public double GetPatternWeighting(List<Note> notes, int sampleRate)
        {
            double temp = 1;
            // Need Mapalyzr Class that identifies patterns
            Mapalyzr mpg = new();

            // TODO: AnalyseSegments
            List<Segment> segments = SegmentAnalyser.AnalyseSegments(notes, sampleRate);

            foreach (Segment segment in segments)
            {
                Console.WriteLine(segment.ToString());
            }

            // TODO: mpg.IdentifyPatterns(segments)

            // TODO: CalculateScoresFromPatterns(patterns)

            return temp;
        }


        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int sampleRate)
        {
            dynamic config = ConfigReader.GetConfig();
            int sampleWindowSecs = int.Parse(config["sample_window_secs"]);

            List<List<Note>> sections = CreateSections(notes, sampleWindowSecs, sampleRate);

            int movingAverageWindow = int.Parse(config["moving_avg_window"]);

            List<double> movingAvg = MovingAverageNoteDensity(sections, movingAverageWindow);

            if (outfile != null)
            {
                foreach (var s in movingAvg)
                {
                    outfile.WriteLine($"{s}");
                }
            }

            double difficulty = WeightedAverageOfValues(movingAvg);

            double weighting = GetPatternWeighting(notes, sampleRate);

            // TODO: Implement this
            WeightingResults weightResults = new();

            // Temp
            weightResults.Weighting = 1;
            weightResults.Difficulty = 1;
            weightResults.WeightedDifficulty = 1;

            return weightResults;
        }
    }

}