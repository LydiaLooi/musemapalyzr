namespace MuseMapalyzr
{
    public class DifficultyCalculation
    {
        public class WeightingResults // Named Tuple "Weighting" in python code
        {
            public double Weighting;
            public double Difficulty;
            public double WeightedDifficulty;

            public WeightingResults(double weighting, double difficulty, double weightedDifficulty)
            {
                Weighting = weighting;
                Difficulty = difficulty;
                WeightedDifficulty = weightedDifficulty;
            }
        }

        public class PatternScore
        {
            public string PatternName;
            public double Score;
            public bool HasInterval;
            public int TotalNotes;
            public PatternScore(string patternName, double score, bool hasInterval, int totalNotes)
            {
                PatternName = patternName;
                Score = score;
                HasInterval = hasInterval;
                TotalNotes = totalNotes;
            }
        }


        /// Creates sections of section_threshold_seconds length to help with calculating
        /// density over the course of a map.
        public List<List<Note>> CreateSections(
            List<Note> notes, int sectionThresholdSeconds = 1, int? sampleRate = null
        )
        {
            if (sampleRate == null) { sampleRate = Constants.DefaultSampleRate; }

            int sectionThreshold = sectionThresholdSeconds * (int)sampleRate;
            double songStartSamples = notes.Min(note => note.SampleTime);
            double songDurationSamples = notes.Max(note => note.SampleTime);

            // Sorting the 'notes' list based on the 'SampleTime' property
            notes = notes.OrderBy(note => note.SampleTime).ToList();

            int numSections = (int)(songDurationSamples - songStartSamples + sectionThreshold) / sectionThreshold;

            List<List<Note>> sections = new List<List<Note>>();

            // Initialize empty sections
            for (int i = 0; i < numSections; i++)
            {
                sections.Add(new List<Note>());
            }

            // Fill sections with notes
            foreach (Note note in notes)
            {
                int sectionIndex = (int)(note.SampleTime - songStartSamples) / sectionThreshold;
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
            // Need Mapalyzr Class that identifies patterns
            Mapalyzr mpg = new();

            List<Segment> segments = SegmentAnalyser.AnalyseSegments(notes, sampleRate);

            List<Pattern> patterns = mpg.IdentifyPatterns(segments);

            List<double> scores = CalculateScoresFromPatterns(patterns);
            double difficulty = DifficultyCalculation.WeightedAverageOfValues(
                scores,
                double.Parse(ConfigReader.GetConfig()["get_pattern_weighting_top_percentage"]),
                double.Parse(ConfigReader.GetConfig()["get_pattern_weighting_top_weight"]),
                double.Parse(ConfigReader.GetConfig()["get_pattern_weighting_bottom_weight"])
                );

            return difficulty;
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

            double weightedDifficulty = weighting * difficulty;

            WeightingResults weightResults = new(weighting, difficulty, weightedDifficulty);



            return weightResults;
        }

        public static List<double> CalculateScoresFromPatterns(List<Pattern> patterns)
        {
            List<PatternScore> patternScores = new List<PatternScore>();
            Console.WriteLine($"Checking {patterns.Count} Patterns");

            foreach (Pattern pattern in patterns)
            {
                Console.WriteLine($"----------{pattern.PatternName} {pattern.Segments.Count}----------");
                if (pattern.Segments != null && pattern.Segments.Count > 0) // check if pattern has segments
                {
                    double score = pattern.CalculatePatternDifficulty();
                    patternScores.Add(
                        new PatternScore(
                            pattern.PatternName,
                            score,
                            pattern.HasIntervalSegment,
                            pattern.TotalNotes
                        )
                    );
                }
            }

            List<double> scores = new List<double>();
            List<PatternScore> chunk = new List<PatternScore>();

            foreach (PatternScore patternScore in patternScores)
            {
                if (patternScore.HasInterval && chunk.Count > 0)
                {
                    List<double> multiplied = ApplyMultiplierToPatternChunk(chunk);
                    scores.AddRange(multiplied);
                    chunk.Clear();
                }
                else
                {
                    chunk.Add(patternScore);
                }
            }

            if (chunk.Count > 0)
            {
                List<double> multiplied = ApplyMultiplierToPatternChunk(chunk);
                scores.AddRange(multiplied);
            }

            return scores;
        }

        public static List<double> ApplyMultiplierToPatternChunk(List<PatternScore> chunk)
        {
            int totalNotes = chunk.Sum(ps => ps.TotalNotes);

            double multiplier = 1;
            if (chunk.Count > 2)
            {
                multiplier = PatternMultiplier.PatternStreamLengthMultiplier(totalNotes);
            }

            List<double> multiplied = new List<double>();
            foreach (PatternScore c_ps in chunk)
            {
                double score = c_ps.PatternName != Constants.ZigZag ? c_ps.Score * multiplier : c_ps.Score;
                multiplied.Add(score);
            }

            Console.WriteLine($"Applying multiplier ({multiplier}x) - Chunk ({totalNotes} notes): {string.Join(", ", chunk)}");

            return multiplied;
        }
    }


}