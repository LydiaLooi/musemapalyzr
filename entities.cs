using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MuseMapalyzr
{

    public class Note
    {
        public int Lane { get; set; }
        public int SampleTime { get; set; }

        public Note(int lane, int sampleTime)
        {
            this.Lane = lane;
            this.SampleTime = sampleTime;
        }

        public override string ToString()
        {
            return $"{Lane}, {SampleTime}";
        }

    }

    public class Segment
    {

        public string SegmentName { get; set; } = "";
        public List<Note> Notes { get; set; } = new List<Note>();

        public int RequiredNotes { get; set; } = 0;

        public int? TimeDifference { get; set; }

        public int SampleRate { get; set; } = Constants.defaultSampleRate;

        public Segment(
            string segmentName,
            List<Note> notes,
            int requiredNotes = 0,
            int? timeDifference = null,
            int? sampleRate = null
            )
        {

            SegmentName = segmentName;
            Notes = notes;
            RequiredNotes = requiredNotes;
            TimeDifference = timeDifference;
            if (sampleRate != null)
            {
                SampleRate = (int)sampleRate;
            }

            if (TimeDifference == null && Notes.Count > 1)
            {
                TimeDifference = Math.Abs(Notes[1].SampleTime - Notes[0].SampleTime);
                Console.WriteLine($"Auto setting time difference to: {TimeDifference}");
            }

        }

        public double GetNotesPerSecond()
        {
            if (TimeDifference == 0)
            {
                return 0;
            }
            if (TimeDifference != null)
            {
                double notesPerSecond = (double)SampleRate / (double)TimeDifference;
                return notesPerSecond;
            }
            Console.WriteLine("Getting NPS failed due to null TimeDifference");
            throw new ArgumentNullException(nameof(TimeDifference));

        }


        public override string ToString()
        {
            return $"{SegmentName} {Notes.Count} {TimeDifference}";
        }

    }

    public class MuseSwiprMap
    {
        public string Title { get; set; } = "";
        public JToken TempoSections { get; set; } = JToken.Parse("{}");
        public JToken Tracks { get; set; } = JToken.Parse("{}");
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