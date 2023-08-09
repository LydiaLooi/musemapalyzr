namespace MuseMapalyzr
{
    public class DifficultyCalculation
    {
        public class WeightingResults // Named Tuple "Weighting" in python code
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


        public class ScoreResults
        {
            public List<double> UnrankedScores;
            public List<double> RankedScores;
            public ScoreResults()
            {
                UnrankedScores = new List<double>();
                RankedScores = new List<double>();
            }
        }

        public class PatternWeightingResults
        {
            public double RankedPatternWeighting;
            public double UnrankedPatternWeighting;
            public PatternWeightingResults(double ranked, double unranked)
            {
                RankedPatternWeighting = ranked;
                UnrankedPatternWeighting = unranked;
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

            // Console.WriteLine(string.Join(",", movingAveragesSorted));

            int thresholdIndex = (int)(values.Count * topPercentage);
            if (thresholdIndex >= movingAveragesSorted.Count)
            {
                thresholdIndex = movingAveragesSorted.Count - 1;
            }
            // Console.WriteLine(thresholdIndex);
            // double threshold = movingAveragesSorted[thresholdIndex];
            // Console.WriteLine($"Top %{topPercentage} Index: {thresholdIndex} ... Threshold {threshold}");

            // Calculate the weighted average
            double totalWeight = 0;
            double weightedSum = 0;

            for (int i = 0; i < movingAveragesSorted.Count; i++)
            {
                double value = movingAveragesSorted[i];
                double weight;
                if (i <= thresholdIndex)
                {
                    weight = topWeight;
                }
                else
                {
                    weight = bottomWeight;
                }

                weightedSum += value * weight;
                totalWeight += weight;
            }
            // Console.WriteLine($"Weighted Sum: {weightedSum} (Actual: {values.Sum()}) Total Weight: {totalWeight}");
            double weightedAverage = weightedSum / totalWeight;
            return weightedAverage;
        }

        public PatternWeightingResults GetPatternWeighting(List<Note> notes, int sampleRate)
        {
            // Need Mapalyzr Class that identifies patterns
            Mapalyzr mpg = new();

            List<Segment> segments = SegmentAnalyser.AnalyseSegments(notes, sampleRate);

            List<Pattern> patterns = mpg.IdentifyPatterns(segments);

            ScoreResults scoreResults = CalculateScoresFromPatterns(patterns);

            double rankedDifficulty = WeightedAverageOfValues(
                scoreResults.RankedScores,
                ConfigReader.GetConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetConfig().GetPatternWeightingBottomWeight
                );

            double unrankedDifficulty = WeightedAverageOfValues(
                scoreResults.UnrankedScores,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingBottomWeight
                );

            return new PatternWeightingResults(rankedDifficulty, unrankedDifficulty);
        }


        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int sampleRate)
        {
            int sampleWindowSecs = ConfigReader.GetConfig().SampleWindowSecs;

            List<List<Note>> sections = CreateSections(notes, sampleWindowSecs, sampleRate);

            int movingAverageWindow = ConfigReader.GetConfig().MovingAvgWindow;

            List<double> movingAvg = MovingAverageNoteDensity(sections, movingAverageWindow);

            if (outfile != null)
            {
                foreach (var s in movingAvg)
                {
                    outfile.WriteLine($"{s}");
                }
            }

            double rankedDifficulty = WeightedAverageOfValues(
                movingAvg,
                ConfigReader.GetConfig().DensityTopProportion,
                ConfigReader.GetConfig().DensityTopWeighting,
                ConfigReader.GetConfig().DensityBottomWeighting
                );
            double unrankedDifficulty = WeightedAverageOfValues(
                movingAvg,
                ConfigReader.GetUnrankedConfig().DensityTopProportion,
                ConfigReader.GetUnrankedConfig().DensityTopWeighting,
                ConfigReader.GetUnrankedConfig().DensityBottomWeighting
                );

            PatternWeightingResults patternWeightingResults = GetPatternWeighting(notes, sampleRate);

            double rankedWeightedDifficulty = patternWeightingResults.RankedPatternWeighting * rankedDifficulty;
            double unrankedWeightedDifficulty = patternWeightingResults.UnrankedPatternWeighting * unrankedDifficulty;

            WeightingResults weightResults = new(
                patternWeightingResults.RankedPatternWeighting,
                rankedDifficulty,
                rankedWeightedDifficulty,

                patternWeightingResults.UnrankedPatternWeighting,
                unrankedDifficulty,
                unrankedWeightedDifficulty
                );


            return weightResults;
        }

        public static ScoreResults CalculateScoresFromPatterns(List<Pattern> patterns)
        {
            List<PatternScore> rankedPatternScores = new List<PatternScore>();
            List<PatternScore> unrankedPatternScores = new List<PatternScore>();
            // Console.WriteLine($"Checking {patterns.Count} Patterns");

            foreach (Pattern pattern in patterns)
            {
                // Console.WriteLine($"----------{pattern.PatternName} {pattern.Segments.Count}----------");
                if (pattern.Segments != null && pattern.Segments.Count > 0) // check if pattern has segments
                {
                    double rankedScore = pattern.CalculatePatternDifficulty(true);
                    double unrankedScore = pattern.CalculatePatternDifficulty(false);
                    // Console.WriteLine($"Pattern Difficulty Score: {score}");
                    rankedPatternScores.Add(
                        new PatternScore(
                            pattern.PatternName,
                            rankedScore,
                            pattern.HasIntervalSegment,
                            pattern.TotalNotes
                        )
                    );
                    unrankedPatternScores.Add(
                        new PatternScore(
                            pattern.PatternName,
                            unrankedScore,
                            pattern.HasIntervalSegment,
                            pattern.TotalNotes
                        )
                    );
                }
            }

            // Before, this was where pattern length multipliers were applied. 
            // That wasn't working as intended so now it's just straight up the scores
            // This is still here incase we want to look into that again.
            ScoreResults scoreResults = new ScoreResults();

            foreach (PatternScore rps in rankedPatternScores)
            {
                scoreResults.RankedScores.Add(rps.Score);
            }

            foreach (PatternScore urps in unrankedPatternScores)
            {
                scoreResults.UnrankedScores.Add(urps.Score);
            }
            return scoreResults;
        }

        // Commented out as it wasn't working as intended and buffing/nerfing maps unintentionally
        // public static List<double> ApplyMultiplierToPatternChunk(List<PatternScore> chunk)
        // {
        //     int totalNotes = chunk.Sum(ps => ps.TotalNotes);

        //     double multiplier = 1;
        //     if (chunk.Count > 2)
        //     {
        //         multiplier = PatternMultiplier.PatternStreamLengthMultiplier(totalNotes);
        //     }

        //     List<double> multiplied = new List<double>();
        //     foreach (PatternScore c_ps in chunk)
        //     {
        //         Console.WriteLine($"Chunk time: {c_ps.PatternName} {c_ps.TotalNotes} ... {multiplier} ... {c_ps.Score}");
        //         double score = c_ps.PatternName != Constants.ZigZag ? c_ps.Score * multiplier : c_ps.Score;
        //         multiplied.Add(score);
        //         if (multiplier > 1)
        //         {
        //             Console.WriteLine($"Before: {c_ps.Score} AFter: {score}");

        //         }
        //     }


        //     return multiplied;
        // }
    }


}