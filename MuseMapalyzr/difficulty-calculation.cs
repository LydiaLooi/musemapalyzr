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
            public List<Pattern> IdentifiedPatterns = new List<Pattern>();
            public List<Segment> IdentifiedSegments = new List<Segment>();
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
                            if (segment.SegmentName == Constants.SingleStreams)
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


        /// Creates sections of section_threshold_seconds length to help with calculating
        /// density over the course of a map.
        public List<List<Note>> CreateSections(
            List<Note> notes,
            int sectionThresholdSeconds,
            int sampleRate,
            List<Segment> streamSegments, // Is ordered 
            double npsCap // E.g., cap is 13 NPS, then if there is a stream thats 42 NPS, we treat it like its 13 NPS.
        )
        {

            int sectionThreshold = sectionThresholdSeconds * sampleRate;
            double songStartSamples = notes.Min(note => note.SampleTime);
            double songDurationSamples = notes.Max(note => note.SampleTime);

            HashSet<double> streamSegmentStartSampleTimes = new HashSet<double>();
            foreach (Segment segment in streamSegments)
            {
                streamSegmentStartSampleTimes.Add(segment.Notes.First().SampleTime);
            }

            // Sorting the 'notes' list based on the 'SampleTime' property
            notes = notes.OrderBy(note => note.SampleTime).ToList();

            int numSections = (int)(songDurationSamples - songStartSamples + sectionThreshold) / sectionThreshold;

            List<List<Note>> sections = new List<List<Note>>();

            // Initialize empty sections
            for (int i = 0; i < numSections; i++)
            {
                sections.Add(new List<Note>());
            }
            Note lastAddedNote = new Note(0, -1);
            // Fill sections with notes
            foreach (Note note in notes)
            {
                int sectionIndex = (int)(note.SampleTime - songStartSamples) / sectionThreshold;
                if (0 <= sectionIndex && sectionIndex < sections.Count)
                {
                    if (streamSegmentStartSampleTimes.Contains(note.SampleTime))
                    {

                        Segment? foundSegment = FindSegmentFromStartNote(note, streamSegments);
                        if (foundSegment == null)
                        {
                            Console.WriteLine("OOF adding anyways");
                            sections[sectionIndex].Add(note);
                            lastAddedNote = note;
                        }
                        else
                        {
                            // Check if Segment NPS is above the threshold or not
                            if (foundSegment.NotesPerSecond > npsCap)
                            {
                                // Console.WriteLine("Found Segment. NPS threshold breached.");
                                sections[sectionIndex].Add(note);

                                Note tempNote = note;
                                // Get the next note it should add:
                                bool done = false;
                                while (!done)
                                {
                                    double nextNoteTime = tempNote.SampleTime + GetTimeDifferenceWithNPS(npsCap, sampleRate);
                                    tempNote = new Note(note.Lane, nextNoteTime);
                                    if (nextNoteTime <= foundSegment.Notes.Last().SampleTime)
                                    {
                                        sections[sectionIndex].Add(tempNote);
                                        lastAddedNote = tempNote;
                                        // Console.WriteLine("Adding note");
                                    }
                                    else
                                    {
                                        // Always Try to add a note at the the last note sample time in the segment
                                        if (lastAddedNote.SampleTime != foundSegment.Notes.Last().SampleTime)
                                        {
                                            Note finalNote = new Note(tempNote.Lane, foundSegment.Notes.Last().SampleTime);
                                            sections[sectionIndex].Add(finalNote);
                                            lastAddedNote = finalNote;
                                            // Console.WriteLine("Added final note");
                                        }
                                        done = true;
                                        // Console.WriteLine("Done");
                                    }
                                }
                            }
                            else
                            {
                                sections[sectionIndex].Add(note);
                                lastAddedNote = note;
                            }
                        }


                    }
                    else if (note.SampleTime > lastAddedNote.SampleTime)
                    {
                        sections[sectionIndex].Add(note);
                        lastAddedNote = note;
                    }

                }
            }

            return sections;
        }

        private double GetTimeDifferenceWithNPS(double NPS, double sampleRate)
        {
            return sampleRate / NPS;
        }

        private Segment? FindSegmentFromStartNote(Note note, List<Segment> segments)
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

            double rankedPatternWeighting = WeightedAverageOfValues(
                scoreResults.RankedScores,
                ConfigReader.GetConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetConfig().GetPatternWeightingBottomWeight
                );

            double unrankedPatternWeighting = WeightedAverageOfValues(
                scoreResults.UnrankedScores,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingBottomWeight
                );
            PatternWeightingResults results = new PatternWeightingResults
            {
                RankedPatternWeighting = rankedPatternWeighting,
                UnrankedPatternWeighting = unrankedPatternWeighting,
                IdentifiedPatterns = patterns,
                IdentifiedSegments = segments
            };

            return results;
        }


        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int sampleRate)
        {

            PatternWeightingResults patternWeightingResults = GetPatternWeighting(notes, sampleRate);

            // foreach (Segment segment in patternWeightingResults.StreamSegments)
            // {
            //     Console.WriteLine(segment.ToString());
            // }

            int sampleWindowSecs = ConfigReader.GetConfig().SampleWindowSecs;

            List<List<Note>> sections = CreateSections(notes, sampleWindowSecs, sampleRate, patternWeightingResults.StreamSegments, 13);

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