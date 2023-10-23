using System.Collections;
using System.Diagnostics;
using System.Text;

namespace MuseMapalyzr
{
    public class DifficultyCalculation
    {
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

                        Segment? foundSegment = Utils.FindSegmentFromStartNote(note, streamSegments);
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



                                    double nextNoteTime = tempNote.SampleTime + Utils.GetTimeDifferenceWithNPS(npsCap, sampleRate);
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







        public PatternWeightingResults GetPatternWeighting(List<Note> notes, int sampleRate)
        {
            // Need Mapalyzr Class that identifies patterns
            Mapalyzr mpg = new Mapalyzr();

            List<Segment> segments = SegmentAnalyser.AnalyseSegments(notes, sampleRate);

            List<Pattern> patterns = mpg.IdentifyPatterns(segments);

            SegmentMultipliers segmentMultipliers = CalculateSegmentMultipliers(patterns);

            double rankedPatternWeighting = Utils.WeightedAverageOfValues(
                segmentMultipliers.RankedMultipliers,
                ConfigReader.GetConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetConfig().GetPatternWeightingBottomWeight
                );

            double unrankedPatternWeighting = Utils.WeightedAverageOfValues(
                segmentMultipliers.PeakMultipliers,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopPercentage,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingTopWeight,
                ConfigReader.GetUnrankedConfig().GetPatternWeightingBottomWeight
                );

            PatternWeightingResults results = new PatternWeightingResults
            {
                RankedPatternWeighting = rankedPatternWeighting,
                UnrankedPatternWeighting = unrankedPatternWeighting,
                IdentifiedPatterns = patterns,
                IdentifiedSegments = segments,
                SegmentMultipliers = segmentMultipliers
            };

            return results;
        }


        public MapDetails AnalyzeMap(List<Note> notes, int sampleRate)
        {
            MapDetails mapDetails = new MapDetails(notes, sampleRate);
            PatternWeightingResults patternWeightingResults = GetPatternWeighting(notes, sampleRate);
            int sampleWindowSecs = 1; // Should not change or will mess up with NPS calculations.

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
            mapDetails.RankedDensities = rankedMovingAvg;
            List<double> unrankedMovingAvg = MovingAverageNoteDensity(unrankedSections, unrankedMovingAverageWindow);
            mapDetails.PeakDensities = unrankedMovingAvg;

            mapDetails.AnalysedPatterns = patternWeightingResults.IdentifiedPatterns;
            mapDetails.AnalysedSegments = patternWeightingResults.IdentifiedSegments;
            mapDetails.CalculateRankedAndPeakValues(patternWeightingResults);

            return mapDetails;
        }


        // When copying over to museswipr, we just get rid of the outfile param.
        public WeightingResults CalculateDifficulty(List<Note> notes, int sampleRate)
        {

            MapDetails mapDetails = AnalyzeMap(notes, sampleRate);

            WeightingResults weightResults = new WeightingResults(
                -1,
                -1,
                mapDetails.RankedDifficulty,

                -1,
                -1,
                mapDetails.PeakDifficulty
                );


            return weightResults;
        }

        public static SegmentMultipliers CalculateSegmentMultipliers(List<Pattern> patterns)
        {
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

            SegmentMultipliers segmentMultipliers = new SegmentMultipliers();

            foreach (Pattern pattern in patterns)
            {
                foreach (Segment segment in pattern.Segments)
                {
                    segmentMultipliers.RankedMultipliers.Add(segment.RankedMultiplier);
                    segmentMultipliers.PeakMultipliers.Add(segment.UnrankedMultiplier);
                }
            }
            return segmentMultipliers;
        }
    }


}