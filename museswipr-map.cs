using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MuseMapalyzr
{

    public class Track
    {
        public string mEventID { get; set; }
        public List<NoteTimeData> mEventList { get; set; }
    }

    public class NoteTimeData
    {
        public int mStartSample { get; set; }
        public int mEndSample { get; set; }
    }

    public class MuseSwiprMap
    {
        public string Title { get; set; }
        public JToken TempoSections { get; set; }
        public JToken Tracks { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();
        public int SampleRate { get; set; }

        public static MuseSwiprMap FromKoreographAsset(string koreographAssetFilename)
        {
            MuseSwiprMap museMap = new();

            using StreamReader file = File.OpenText(koreographAssetFilename);
            using JsonTextReader reader = new(file);
            JObject data = (JObject)JToken.ReadFrom(reader);

            // Assuming that the data has at least one key
            var firstKey = data.Properties().First().Name;

            museMap.Title = firstKey;
            museMap.TempoSections = data[firstKey]["value"]["mTempoSections"];
            museMap.Tracks = data[firstKey]["value"]["mTracks"];
            museMap.SampleRate = (int)data[firstKey]["value"]["mSampleRate"];

            museMap.ParseNotes();

            return museMap;

        }

        public void OutputNotes(string filePath) // WHO CARES DON"T DEAL WITH THIS.
        {
            Console.WriteLine("Outputting notes");
            string mapOutput = Constants.mapOutputs;

            List<Note> notes = this.Notes;

            // Find the smallest time distance between notes
            int smallestTimeDistance = notes.Zip(notes.Skip(1), (note1, note2) => note2.SampleTime - note1.SampleTime).Min();

            // Sort the notes by descending time order
            List<Note> sortedNotes = notes.OrderByDescending(note => note.SampleTime).ToList();

            // Get the range of time values to iterate over
            int startTime = sortedNotes.Last().SampleTime;
            int endTime = sortedNotes.First().SampleTime + smallestTimeDistance;
            List<int> timeRange = Enumerable.Range(startTime, endTime - startTime).ToList();

            Console.WriteLine($"Smallest time distance: {smallestTimeDistance}");

            // Write the output to the file
            foreach (Note note in sortedNotes) {
                Console.WriteLine($"{note.SampleTime} {note.Lane}");
            }
            // using (StreamWriter file = new StreamWriter(Path.Combine(mapOutput, filePath)))
            // {
            //     foreach (int time in timeRange.AsEnumerable().Reverse())
            //     {
            //         foreach (Note note in sortedNotes)
            //         {
            //             // Console.WriteLine($"Note {note.SampleTime} Lane {note.Lane} Event ID {note.Lane} Sample time: {note.SampleTime}");
            //             if (note.SampleTime <= time && time < note.SampleTime + smallestTimeDistance)
            //             {
            //                 if (note.Lane == 0)
            //                 {
            //                     file.WriteLine($"{(double)note.SampleTime / this.SampleRate:F2}| [0]");
            //                 }
            //                 else if (note.Lane == 1)
            //                 {
            //                     file.WriteLine($"{(double)note.SampleTime / this.SampleRate:F2}|      [1]");
            //                 }
            //                 break;
            //             }
            //         }
            //         file.WriteLine($"{(double)time / this.SampleRate:F2}|");
            //     }
            // }
        }

        public void ParseNotes()
        {
            foreach (JToken t in this.Tracks)
            {
                JArray noteTimesData = (JArray)t["mEventList"];
                foreach (JToken d in noteTimesData)
                {
                    if ((int)d["mStartSample"] != (int)d["mEndSample"])
                    {
                        throw new Exception("mStartSample and mEndSample are not equal");
                    }
                    if ((string)t["mEventID"] != "TimingPoint")
                    {
                        Note note = new(int.Parse((string)t["mEventID"]), (int)d["mStartSample"]);
                        this.Notes.Add(note);
                    }
                }
            }

            this.Notes = this.Notes.OrderBy(note => note.SampleTime).ToList();
        }

    }
}