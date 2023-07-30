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
            if (sampleRate == null) { sampleRate = Constants.DEFAULT_SAMPLE_RATE; }

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

        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int sampleRate)
        {
            dynamic config = ConfigReader.GetConfig();
            int sampleWindowSecs = int.Parse(config["sample_window_secs"]);

            List<List<Note>> sections = CreateSections(notes, sampleWindowSecs, sampleRate);

            foreach (List<Note> listOfNotes in sections)
            {
                int startSampleTime = -1;
                if (listOfNotes.Count > 0)
                {
                    startSampleTime = listOfNotes[0].SampleTime;
                }
                Console.WriteLine(listOfNotes.Count + "   " + startSampleTime);
            }

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