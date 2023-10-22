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
    public class DifficultyCalculation
    {


        private static float BaseDifficultyMultiplier = 0.5f; // So that the difficulty range is somewhat more palatable

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


        private void PutPatternsIntoSections(
            List<Segment> segmentsWithMultipliers,
            double songStartSamples,
            int sectionThreshold,
            PatternWeightingResults patternWeightingResults,
            int sampleRate
            )
        {
            int sectionsCount = patternWeightingResults.RankedPatternWeightingSections.Count;
            foreach (Segment segment in segmentsWithMultipliers)
            {
                int firstNoteSectionIndex = (int)(segment.Notes.First().SampleTime - songStartSamples) / sectionThreshold;
                if (0 <= firstNoteSectionIndex && firstNoteSectionIndex < sectionsCount)
                {
                    patternWeightingResults.RankedPatternWeightingSections[firstNoteSectionIndex].Add(segment.RankedMultiplier);
                    patternWeightingResults.UnrankedPatternWeightingSections[firstNoteSectionIndex].Add(segment.UnrankedMultiplier);

                    // Now check the section index of the final note.
                    int lastNoteSectionIndex = (int)(segment.Notes.Last().SampleTime - songStartSamples) / sectionThreshold;
                    // If the last note's section index is more than +1 compared to the first note's section index
                    if (lastNoteSectionIndex > firstNoteSectionIndex + 1)
                    {
                        // Fill in the section indexes up to but not including the last note section index
                        for (int i = firstNoteSectionIndex + 1; i < lastNoteSectionIndex; i++)
                        {
                            if (i < sectionsCount) // Safety check
                            {
                                patternWeightingResults.RankedPatternWeightingSections[i].Add(segment.RankedMultiplier);
                                patternWeightingResults.UnrankedPatternWeightingSections[i].Add(segment.UnrankedMultiplier);
                            }
                        }
                    }


                }
            }
        }

        private void PutNotesIntoSections(
            List<Note> notes,
            double songStartSamples,
            int sectionThreshold,
            List<List<Note>> sections,
            List<Segment> streamSegments,
            double streamNpsCap,
            double fourStackNpsCap,
            double threeStackNpsCap,
            double twoStackNpsCap,
            int sampleRate

            )
        {


            int skipped = 0;
            // Note lastAddedNote = new Note(0, -1);
            double skipIfBeforeOrEqualToThisTime = 0;

            HashSet<double> streamSegmentStartSampleTimes = new HashSet<double>();
            foreach (Segment segment in streamSegments)
            {
                streamSegmentStartSampleTimes.Add(segment.Notes.First().SampleTime);
            }

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

                            if (skipped > 0) CustomLogger.Instance.Debug($"Found segment == null: Skipped {skipped}");
                            skipped = 0;
                        }
                        else
                        {

                            List<Note> segmentNotes = foundSegment.Notes.OrderBy(note => note.SampleTime).ToList();
                            double npsCap;

                            switch (foundSegment.SegmentName)
                            {
                                case Constants.SingleStreams:
                                    npsCap = streamNpsCap;
                                    CustomLogger.Instance.Debug($"NPS Cap: stream cap: {streamNpsCap}");
                                    break;
                                case Constants.FourStack:
                                    npsCap = fourStackNpsCap;
                                    CustomLogger.Instance.Debug($"NPS Cap: 4-stack cap: {fourStackNpsCap}");
                                    break;
                                case Constants.ThreeStack:
                                    npsCap = threeStackNpsCap;
                                    CustomLogger.Instance.Debug($"NPS Cap: 3-stack cap: {threeStackNpsCap}");
                                    break;
                                case Constants.TwoStack:
                                    npsCap = twoStackNpsCap;
                                    CustomLogger.Instance.Debug($"NPS Cap: 2-stack cap: {twoStackNpsCap}");
                                    break;
                                default:
                                    CustomLogger.Instance.Error($"Create sections stream segments found a non stream or N-stack segment? {foundSegment.SegmentName}");
                                    npsCap = streamNpsCap;
                                    break;
                            }


                            if (skipped > 0) CustomLogger.Instance.Debug($"Found a stream. Skipped {skipped}");
                            skipped = 0;
                            // Check if Segment NPS is above the threshold or not
                            if (foundSegment.NotesPerSecond > npsCap)
                            {
                                CustomLogger.Instance.Debug($"Found a {foundSegment.SegmentName} with {foundSegment.Notes.Count} notes and {foundSegment.NotesPerSecond:F3} NPS");
                                CustomLogger.Instance.Debug($"{segmentNotes.First().SampleTime} -> {segmentNotes.Last().SampleTime}");

                                skipIfBeforeOrEqualToThisTime = foundSegment.Notes.Last().SampleTime;




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
                                        notesAdded++;
                                    }
                                    else
                                    {
                                        done = true;
                                        CustomLogger.Instance.Debug($"Done: {notesAdded} added");
                                    }
                                }
                            }
                            else // The Segment NPS was not above the threshold so we just add it as normal
                            {
                                sections[sectionIndex].Add(note);
                            }
                        }


                    }
                    else if (note.SampleTime > skipIfBeforeOrEqualToThisTime)
                    {
                        sections[sectionIndex].Add(note);
                        if (skipped > 0) CustomLogger.Instance.Debug($"note.SampleTime > lastAddedNote.SampleTime: Skipped {skipped}");
                        skipped = 0;

                    }
                    else
                    {
                        skipped++;
                    }

                }
            }

        }


        /// Creates sections of section_threshold_seconds length to help with calculating
        /// density over the course of a map.
        /// Also creates the sections for pattern weightings
        public SectionResults CreateSections(
            List<Note> notes,
            int sectionThresholdSeconds,
            int sampleRate,
            PatternWeightingResults patternWeightingResults
        // double streamNpsCap, // E.g., cap is 13 NPS, then if there is a stream thats 42 NPS, we treat it like its 13 NPS.
        )
        {

            List<Segment> streamSegments = patternWeightingResults.StreamSegments;  // Is ordered 


            double rankedStreamNpsCap = ConfigReader.GetConfig().DensitySingleStreamNPSCap;
            double rankedFourStackNpsCap = ConfigReader.GetConfig().DensityFourStackNPSCap;
            double rankedThreeStackNpsCap = ConfigReader.GetConfig().DensityThreeStackNPSCap;
            double rankedTwoStackNpsCap = ConfigReader.GetConfig().DensityTwoStackNPSCap;
            double rankedNormalSizedMapThreshold = ConfigReader.GetConfig().NormalSizedMapThreshold;

            double unrankedStreamNpsCap = ConfigReader.GetUnrankedConfig().DensitySingleStreamNPSCap;
            double unrankedFourStackNpsCap = ConfigReader.GetUnrankedConfig().DensityFourStackNPSCap;
            double unrankedThreeStackNpsCap = ConfigReader.GetUnrankedConfig().DensityThreeStackNPSCap;
            double unrankedTwoStackNpsCap = ConfigReader.GetUnrankedConfig().DensityTwoStackNPSCap;
            //double unrankedNormalSizedMapThreshold = ConfigReader.GetUnrankedConfig().NormalSizedMapThreshold;

            List<Note> rankedNotes = new List<Note>(notes);

            double songLength = (notes.Last().SampleTime - notes.First().SampleTime) / sampleRate;
            if (songLength < rankedNormalSizedMapThreshold)
            {
                // If the song length is less than the threshold, then add a note at the threshold time
                // so that when calculating density, it looks at that whole time.
                rankedNotes.Add(new Note(0, rankedNormalSizedMapThreshold * sampleRate));
            }


            int sectionThreshold = sectionThresholdSeconds * sampleRate;
            double songStartSamples = notes.Min(note => note.SampleTime);
            double songDurationSamples = notes.Max(note => note.SampleTime);

            // Sorting the 'notes' list based on the 'SampleTime' property
            notes = notes.OrderBy(note => note.SampleTime).ToList();

            int numSections = (int)(songDurationSamples - songStartSamples + sectionThreshold) / sectionThreshold;

            List<List<Note>> rankedSections = new List<List<Note>>();
            List<List<Note>> unrankedSections = new List<List<Note>>();

            List<List<double>> rankedPatternWeightingSections = new List<List<double>>();
            List<List<double>> unrankedPatternWeightingSections = new List<List<double>>();

            // Initialize empty sections
            for (int i = 0; i < numSections; i++)
            {
                rankedSections.Add(new List<Note>());
                unrankedSections.Add(new List<Note>());

                rankedPatternWeightingSections.Add(new List<double>());
                unrankedPatternWeightingSections.Add(new List<double>());
            }

            patternWeightingResults.RankedPatternWeightingSections = rankedPatternWeightingSections;
            patternWeightingResults.UnrankedPatternWeightingSections = unrankedPatternWeightingSections;

            // Fill sections with notes
            PutNotesIntoSections(
                notes,
                songStartSamples,
                sectionThreshold,
                rankedSections,
                streamSegments,
                rankedStreamNpsCap,
                rankedFourStackNpsCap,
                rankedThreeStackNpsCap,
                rankedTwoStackNpsCap,
                sampleRate
                );

            PutNotesIntoSections(
                notes,
                songStartSamples,
                sectionThreshold,
                unrankedSections,
                streamSegments,
                unrankedStreamNpsCap,
                unrankedFourStackNpsCap,
                unrankedThreeStackNpsCap,
                unrankedTwoStackNpsCap,
                sampleRate
                );

            PutPatternsIntoSections(
                patternWeightingResults.IdentifiedSegments,
                songStartSamples,
                sectionThreshold,
                patternWeightingResults,
                sampleRate
            );


            return new SectionResults(rankedSections, unrankedSections);
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


        public static double CalculateDensityAverage(
            List<double> values,
            int hardestSeconds,
            double arbitrary90PercentThreshold,
            double rankedPenaltyProportion,
            double ceilingProportion,
            List<List<double>> patternMultiplierSections
            )
        {

            // Values should be the densities * pattern multipliers in each 1 second windows

            // Check if the list is empty
            if (values.Count == 0 || values == null)
            {
                throw new ArgumentException("Input list is empty.");
            }

            int numTopValues = hardestSeconds;


            //Console.WriteLine($"DENSITY LENGTH: {values.Count}");

            // Console.WriteLine(values.Count == patternMultiplierSections.Count);

            int index = 0;
            foreach (List<double> patternMultipliers in patternMultiplierSections)
            {
                if (patternMultipliers.Count == 0)
                {
                    patternMultipliers.Add(1);
                }
                // double averagePatternMultiplier = patternMultipliers.Average();
                double averagePatternMultiplier = WeightedAverageOfValues(patternMultipliers, 0.2, 0.9, 0.1);
                double before = values[index];
                values[index] *= averagePatternMultiplier;
                // Console.WriteLine($"Index: {index} Count: {patternMultipliers.Count} | Before: {before} After: {values[index]} {averagePatternMultiplier}x");
                index++;

            }


            // Sort the list in descending order
            values = values.OrderByDescending(d => d).ToList();

            // Console.WriteLine(string.Join(",", values));



            // IEnumerable<double> something = values.Take(numTopValues);
            // foreach (double s in something)
            // {
            //     Console.WriteLine(s);
            // }
            // Console.WriteLine("END");


            List<double> topValues = values.Take(numTopValues).ToList();
            double hardest = WeightedAverageOfValues(topValues, 0.2, 0.9, 0.1);

            double additionalStars = ceilingProportion * hardest;

            // Console.WriteLine(additionalStars + " Additional stars");
            double ceiling = hardest + additionalStars;



            double cumulativeSumOfDensities = CalculateWeightedSum(values, hardest);

            double penalty = hardest * rankedPenaltyProportion; // 0 if calculating peak difficulty
            double finalPenalisedBase = hardest - penalty; // Is just hardest if calculating peak difficulty

            double X = ceiling - finalPenalisedBase; // The number to approach
            double N = arbitrary90PercentThreshold;  // The point where the function should be 90% of X
            double addedDifficulty = LogarithmicGrowth(cumulativeSumOfDensities, X, N);



            double finalDifficulty = finalPenalisedBase + addedDifficulty;
            Console.WriteLine($"X: {X} | added diff: {addedDifficulty}");
            Console.WriteLine($"Final diff: {finalDifficulty} | ... {rankedPenaltyProportion} Ceiling: {ceiling} | Hardest: {hardest} | final penalised: {finalPenalisedBase} | Cumulative sum: {cumulativeSumOfDensities}");



            return finalDifficulty;
        }


        private static double LogarithmicGrowth(double x, double X, double N)
        {
            double b = (Math.Exp(0.9) - 1) / N;
            return X * Math.Log(b * x + 1);
        }

        private static double CalculateWeightedSum(IEnumerable<double> bottomNums, double hardest)
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

        public PatternWeightingResults GetPatternWeighting(List<Note> notes, int sampleRate)
        {
            // Need Mapalyzr Class that identifies patterns
            Mapalyzr mpg = new Mapalyzr();

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


        // When copying over to museswipr, we just get rid of the outfile param.
        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int sampleRate)
        {

            PatternWeightingResults patternWeightingResults = GetPatternWeighting(notes, sampleRate);

            int sampleWindowSecs = ConfigReader.GetConfig().SampleWindowSecs;

            SectionResults sectionResults = CreateSections(
                notes.ToList(),
                sampleWindowSecs,
                sampleRate,
                patternWeightingResults
                );


            List<List<Note>> unrankedSections = sectionResults.UnrankedSections;
            List<List<Note>> rankedSections = sectionResults.RankedSections;

            int movingAverageWindow = ConfigReader.GetConfig().MovingAvgWindow;
            int unrankedMovingAverageWindow = ConfigReader.GetUnrankedConfig().MovingAvgWindow;

            List<double> rankedMovingAvg = MovingAverageNoteDensity(rankedSections, movingAverageWindow);
            List<double> unrankedMovingAvg = MovingAverageNoteDensity(unrankedSections, unrankedMovingAverageWindow);

            // When copying over to museswipr, we just get rid of the if below.
            if (outfile != null)
            {
                foreach (var s in rankedMovingAvg)
                {
                    outfile.WriteLine($"{s}");
                }
            }

            double rankedDifficulty = CalculateDensityAverage(
                rankedMovingAvg,
                ConfigReader.GetConfig().HardestSeconds,
                ConfigReader.GetConfig().Arbitrary90PercentThreshold,
                ConfigReader.GetConfig().RankedPenaltyProportion,
                ConfigReader.GetConfig().CeilingProportion,
                patternWeightingResults.RankedPatternWeightingSections
                ) * BaseDifficultyMultiplier;



            double unrankedDifficulty = CalculateDensityAverage(
                unrankedMovingAvg,
                ConfigReader.GetUnrankedConfig().HardestSeconds,
                ConfigReader.GetUnrankedConfig().Arbitrary90PercentThreshold,
                ConfigReader.GetUnrankedConfig().RankedPenaltyProportion,
                ConfigReader.GetUnrankedConfig().CeilingProportion,
                patternWeightingResults.UnrankedPatternWeightingSections
                ) * BaseDifficultyMultiplier;


            // The unranked difficulty should never be lower than the ranked difficulty
            unrankedDifficulty = Math.Max(rankedDifficulty, unrankedDifficulty);
            Console.WriteLine($"Unranked: {unrankedDifficulty} Ranked: {rankedDifficulty}");
            double rankedWeightedDifficulty = rankedDifficulty;
            double unrankedWeightedDifficulty = unrankedDifficulty;

            WeightingResults weightResults = new WeightingResults(
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
            CustomLogger.Instance.PatternLog(String.Format("\n{0,20} {1,10} {2,18} {3,18}", "Pattern Name", "# Segments", "Start SampleTime", "End SampleTime"));
            foreach (Pattern pattern in patterns)
            {
                // Console.WriteLine($"----------{pattern.PatternName} {pattern.Segments.Count}----------");
                if (pattern.Segments != null && pattern.Segments.Count > 0) // check if pattern has segments
                {

                    pattern.CalculatePatternDifficulty(true);
                    pattern.CalculatePatternDifficulty(false);


                    CustomLogger.Instance.PatternLog($"{pattern.PatternName,20} {pattern.Segments.Count,10} {pattern.StartSample,18} {pattern.EndSample,18}");
                    if (true)
                    {
                        CustomLogger.Instance.PatternLog(String.Format("\n{0,30} {1,10} {2,10} {3,10} {4,10}", "Segment Name", "# Notes", "NPS", "R Multi", "UR Multi"));

                        foreach (Segment seg in pattern.Segments)
                        {
                            CustomLogger.Instance.PatternLog($"{seg.SegmentName,30} {seg.Notes.Count,10} {seg.NotesPerSecond,10:F3} {seg.RankedMultiplier,10:F3} {seg.UnrankedMultiplier,10:F3}");

                        }
                        CustomLogger.Instance.PatternLog("");

                    }
                }
            }

            ScoreResults scoreResults = new ScoreResults();

            foreach (Pattern pattern in patterns)
            {
                foreach (Segment segment in pattern.Segments)
                {
                    scoreResults.RankedScores.Add(segment.RankedMultiplier);
                    scoreResults.UnrankedScores.Add(segment.UnrankedMultiplier);
                }
            }
            return scoreResults;
        }
    }


}