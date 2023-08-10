using System.Text;

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

            int skipped = 0;
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
                            CustomLogger.Instance.Warning($"Didn't actually find a segment.. Adding the note anyways SampleTime{note.SampleTime}");
                            sections[sectionIndex].Add(note);
                            lastAddedNote = note;

                            if (skipped > 0) CustomLogger.Instance.Debug($"Found segment == null: Skipped {skipped}");
                            skipped = 0;
                        }
                        else
                        {

                            List<Note> segmentNotes = foundSegment.Notes.OrderBy(note => note.SampleTime).ToList();

                            if (skipped > 0) CustomLogger.Instance.Debug($"Found a stream. Skipped {skipped}");
                            skipped = 0;
                            // Check if Segment NPS is above the threshold or not
                            if (foundSegment.NotesPerSecond > npsCap)
                            {
                                CustomLogger.Instance.Debug($"Found Segment. Count: ({foundSegment.Notes.Count}) NPS: {foundSegment.NotesPerSecond}");
                                CustomLogger.Instance.Debug($"{segmentNotes.First().SampleTime} -> {segmentNotes.Last().SampleTime}");

                                int notesAdded = 0;

                                sections[sectionIndex].Add(note);
                                notesAdded++;


                                Note tempNote = note;
                                // Get the next note it should add:
                                bool done = false;
                                while (!done)
                                {
                                    double nextNoteTime = tempNote.SampleTime + GetTimeDifferenceWithNPS(npsCap, sampleRate);
                                    int nextSectionIndex = (int)(nextNoteTime - songStartSamples) / sectionThreshold;
                                    tempNote = new Note(note.Lane, nextNoteTime);
                                    if (nextNoteTime <= segmentNotes.Last().SampleTime)
                                    {
                                        sections[nextSectionIndex].Add(tempNote);
                                        lastAddedNote = tempNote;
                                        notesAdded++;
                                        // CustomLogger.Instance.Debug($"Adding note: {tempNote.SampleTime}");
                                    }
                                    else
                                    {
                                        // Always Try to add a note at the the last note sample time in the segment
                                        if (lastAddedNote.SampleTime != segmentNotes.Last().SampleTime)
                                        {
                                            Note finalNote = new Note(tempNote.Lane, segmentNotes.Last().SampleTime);
                                            int finalSectionIndex = (int)(finalNote.SampleTime - songStartSamples) / sectionThreshold;
                                            sections[finalSectionIndex].Add(finalNote);
                                            lastAddedNote = finalNote;
                                            notesAdded++;
                                            // CustomLogger.Instance.Debug($"Added final note: {finalNote.SampleTime}");
                                        }
                                        done = true;
                                        CustomLogger.Instance.Debug($"Done: {notesAdded} added");
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
                        if (skipped > 0) CustomLogger.Instance.Debug($"note.SampleTime > lastAddedNote.SampleTime: Skipped {skipped}");
                        skipped = 0;

                    }
                    else
                    {
                        skipped++;
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

            CustomLogger.Instance.Debug($"MovingAverageNoteDensity Num Sections: {numSections} WindowSize: {windowSize}");

            List<int> noteDensities = new List<int>();
            foreach (var section in sections)
            {
                noteDensities.Add(section.Count);
                CustomLogger.Instance.Debug($"Adding {section.Count} to noteDensities");
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

            int sampleWindowSecs = ConfigReader.GetConfig().SampleWindowSecs;

            // TODO: This could be optimised - instead of running CreateSections twice, just do both ranked and 
            // unranked in it.
            List<List<Note>> rankedSections = CreateSections(
                notes,
                sampleWindowSecs,
                sampleRate,
                patternWeightingResults.StreamSegments,
                ConfigReader.GetConfig().DensitySingleStreamNPSCap
                );

            List<List<Note>> unrankedSections = CreateSections(
                notes,
                sampleWindowSecs,
                sampleRate,
                patternWeightingResults.StreamSegments,
                ConfigReader.GetUnrankedConfig().DensitySingleStreamNPSCap
                );

            int movingAverageWindow = ConfigReader.GetConfig().MovingAvgWindow;

            List<double> rankedMovingAvg = MovingAverageNoteDensity(rankedSections, movingAverageWindow);
            List<double> unrankedMovingAvg = MovingAverageNoteDensity(unrankedSections, movingAverageWindow);

            if (outfile != null)
            {
                foreach (var s in rankedMovingAvg)
                {
                    outfile.WriteLine($"{s}");
                }
            }

            double rankedDifficulty = WeightedAverageOfValues(
                rankedMovingAvg,
                ConfigReader.GetConfig().DensityTopProportion,
                ConfigReader.GetConfig().DensityTopWeighting,
                ConfigReader.GetConfig().DensityBottomWeighting
                );
            double unrankedDifficulty = WeightedAverageOfValues(
                unrankedMovingAvg,
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