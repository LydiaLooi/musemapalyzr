using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MuseMapalyzr
{

    public class NoteTimeData
    {
        public int mStartSample { get; set; }
        public int mEndSample { get; set; }
    }

    public class MuseSwiprMap
    {
        public string Title { get; set; } = "";
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