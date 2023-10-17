using System;
using System.Collections.Generic;

namespace MuseMapalyzr
{
    public class SegmentAnalyser
    {
        private static int MaxNumberNotesInSegment = 25; // So that long zig zags are taken into account more.

        private static bool IfZigZagItIsValid(Segment currentSegment)
        {
            if (currentSegment.SegmentName != Constants.ZigZag) return true;

            if (currentSegment.Notes.Count < MaxNumberNotesInSegment) return true;

            return false;
        }

        public static List<Segment> AnalyseSegments(List<Note> notes, int sampleRate)
        {
            List<Segment> segments = new List<Segment>();
            Segment? currentSegment = null;
            double tolerance = ConfigReader.GetConfig().SegmentToleranceMs * sampleRate / 1000;  // 10ms in sample time

            for (int i = 1; i < notes.Count; i++)  // Starts at second note
            {
                Note prevNote = notes[i - 1];
                Note note = notes[i];

                double timeDifference = note.SampleTime - prevNote.SampleTime;

                // Get the name of the next segment and the notes required to complete it
                (string nextSegmentName, int nextRequiredNotes) = GetNextSegmentAndRequiredNotes(prevNote, note, timeDifference, sampleRate);

                // If the current pair of notes belongs to the same segment as the previous pair of notes
                if (currentSegment != null && currentSegment.SegmentName == nextSegmentName)
                {
                    double baseTimeDifference = currentSegment.Notes[1].SampleTime - currentSegment.Notes[0].SampleTime;

                    // If the time difference between the current pair of notes is within the tolerance of the base time difference of the current segment
                    if (Math.Abs(timeDifference - baseTimeDifference) <= tolerance && IfZigZagItIsValid(currentSegment))
                    {
                        currentSegment.Notes.Add(note);
                    }
                    else
                    {
                        segments = HandleCurrentSegment(segments, currentSegment);
                        currentSegment = new Segment(nextSegmentName, new List<Note> { prevNote, note }, nextRequiredNotes, timeDifference, sampleRate);
                    }
                }
                else
                {

                    segments = HandleCurrentSegment(segments, currentSegment);
                    currentSegment = new Segment(nextSegmentName, new List<Note> { prevNote, note }, nextRequiredNotes, timeDifference, sampleRate);


                }
            }

            segments = HandleCurrentSegment(segments, currentSegment);

            return segments;
        }

        public static List<Segment> HandleCurrentSegment(List<Segment> segments, Segment? currentSegment)
        {
            if (currentSegment != null)
            {
                if (currentSegment.SegmentName == Constants.Other)
                {
                    segments.Add(currentSegment);
                }
                else if (currentSegment.Notes.Count >= currentSegment.RequiredNotes)
                {
                    if (currentSegment.SegmentName == Constants.ZigZag && currentSegment.Notes.Count == 2)
                    {
                        currentSegment.SegmentName = Constants.Switch;
                    }
                    else if (currentSegment.SegmentName == Constants.SingleStreams && currentSegment.Notes.Count < 5)
                    {
                        currentSegment.SegmentName = $"{currentSegment.Notes.Count}-Stack";
                    }
                    segments.Add(currentSegment);
                }
            }
            return segments;
        }
        public static Tuple<string, int> GetNextSegmentAndRequiredNotes(Note prevNote, Note note, double timeDifference, int sampleRate)
        {
            double notesPerSecond = sampleRate / timeDifference;

            if (notesPerSecond >= ConfigReader.GetConfig().ShortIntervalNps)
            {
                if (note.Lane != prevNote.Lane)
                {
                    return new Tuple<string, int>(Constants.ZigZag, 2);
                }
                else
                {
                    return new Tuple<string, int>(Constants.SingleStreams, 2);
                }
            }
            else if (notesPerSecond < ConfigReader.GetConfig().LongIntervalNps)
            {
                return new Tuple<string, int>(Constants.LongInterval, 0);
            }
            else if (notesPerSecond < ConfigReader.GetConfig().MedIntervalNps)
            {
                return new Tuple<string, int>(Constants.MedInterval, 0);
            }
            else if (notesPerSecond < ConfigReader.GetConfig().ShortIntervalNps)
            {
                return new Tuple<string, int>(Constants.ShortInterval, 0);
            }
            else
            {
                return new Tuple<string, int>(Constants.Other, 0);
            }
        }



    }
}