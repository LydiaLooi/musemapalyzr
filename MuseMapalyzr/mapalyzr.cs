namespace MuseMapalyzr
{
    public class Mapalyzr
    {

        public List<Pattern> Patterns { get; set; }
        public bool PatternAppended { get; set; }
        public string PatternAppendedName { get; set; }
        public List<Pattern> Groups { get; set; }
        public OtherPattern OtherPattern { get; set; }
        public bool Added { get; set; }
        public bool Reset { get; set; }


        public Mapalyzr()
        {
            Patterns = new List<Pattern>();
            PatternAppended = false;
            PatternAppendedName = "";
            Groups = new List<Pattern>();
            OtherPattern = new OtherPattern(Constants.Other, new List<Segment>());
            ResetGroups();
            Added = false;
            Reset = false;
        }

        public bool IsNStack(Segment segment)
        {
            List<string> stackNames = new List<string> {
                Constants.TwoStack,
                Constants.ThreeStack,
                Constants.FourStack
                };
            return stackNames.Contains(segment.SegmentName);
        }

        public bool SegmentIsInterval(Segment segment)
        {
            return segment.SegmentName.Contains("Interval");
        }

        public void ResetGroups()
        {
            // Implement the method here.
        }

        public List<Pattern> IdentifyPatterns(List<Segment> segments)
        {
            List<Pattern> pattern = new List<Pattern>();
            return pattern;
        }
    }

}