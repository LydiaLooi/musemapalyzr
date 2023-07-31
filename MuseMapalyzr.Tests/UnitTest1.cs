namespace MuseMapalyzr.Tests;
using MuseMapalyzr;
public class PatternTests
{

    private static List<Note> TwoNotes = new List<Note> { new Note(0, 0), new Note(0, 0) };
    private static Segment ShortInterval = new Segment(Constants.ShortInterval, TwoNotes, 0, 0);
    private static Segment MedInterval = new Segment(Constants.MedInterval, TwoNotes, 0, 0);
    private static Segment LongInterval = new Segment(Constants.LongInterval, TwoNotes, 0, 0);
    private static Segment TwoStack = new Segment(Constants.TwoStack, TwoNotes, 0, 0);
    private static Segment ThreeStack = new Segment(Constants.ThreeStack, TwoNotes, 0, 0);
    private static Segment FourStack = new Segment(Constants.FourStack, TwoNotes, 0, 0);
    private static Segment Switch = new Segment(Constants.Switch, TwoNotes, 0, 0);
    private static Segment ZigZag = new Segment(Constants.ZigZag, TwoNotes, 0, 0);
    private static Segment SingleStreams = new Segment(Constants.SingleStreams, TwoNotes, 0, 0);

    private Note CreateNote(int lane, double seconds)
    {
        return new Note(lane, seconds * Constants.DefaultSampleRate);
    }

    [Fact]
    public void OneIntervalIsOther()
    {
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(new List<Segment> { ShortInterval });

        Assert.Single(groups);
        Assert.Equal(Constants.Other, groups[0].PatternName);
        Assert.Single(groups[0].Segments);
    }

    [Fact]
    public void TwoIntervalsIsSlowStretch()
    {
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(new List<Segment> { ShortInterval, MedInterval });

        Assert.Single(groups);
        Assert.Equal(Constants.SlowStretch, groups[0].PatternName);
        Assert.Equal(2, groups[0].Segments.Count);
    }

    [Fact]
    public void DifferingIntervalsIsSlowStretch()
    {
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(new List<Segment> { ShortInterval, ShortInterval, MedInterval, LongInterval, ShortInterval });

        Assert.Single(groups);
        Assert.Equal(Constants.SlowStretch, groups[0].PatternName);
        Assert.Equal(5, groups[0].Segments.Count);
    }


    [Fact]
    public void VaryingStacksOnly()
    {
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(new List<Segment> { TwoStack, ThreeStack, TwoStack });

        Assert.Single(groups);
        Assert.Equal(Constants.VaryingStacks, groups[0].PatternName);
        Assert.Equal(3, groups[0].Segments.Count);
    }

    [Fact]
    public void VaryingStacksWithIntervals()
    {
        List<Segment> segments = new List<Segment> { ShortInterval, MedInterval, ThreeStack, FourStack, ShortInterval, LongInterval };
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(segments);

        Assert.Equal(Constants.SlowStretch, groups[0].PatternName);
        Assert.Equal(Constants.VaryingStacks, groups[1].PatternName);
        Assert.Equal(Constants.SlowStretch, groups[2].PatternName);
        Assert.Equal(2, groups[0].Segments.Count);
        Assert.Equal(4, groups[1].Segments.Count);  // Includes med_interval and short_interval on either side
        Assert.Equal(2, groups[2].Segments.Count);
    }

    [Fact]
    public void OtherWithOneSegment()
    {
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(new List<Segment> { Switch });

        Assert.Single(groups);
        Assert.Equal(Constants.Other, groups[0].PatternName);
        Assert.Single(groups[0].Segments);
    }



    [Fact]
    public void SimplePatternThenVaryingStacksEndingWithOther()
    {
        List<Segment> segments = new List<Segment> {
            ShortInterval,
            ShortInterval,
            TwoStack,
            ThreeStack,
            TwoStack,
            Switch,
            ZigZag,
            Switch,
            TwoStack,
            ZigZag,
            SingleStreams
            };
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(segments);

        Assert.Equal(3, groups.Count);

        Assert.Equal(Constants.SlowStretch, groups[0].PatternName);
        Assert.Equal(2, groups[0].Segments.Count);

        Assert.Equal(Constants.VaryingStacks, groups[1].PatternName);
        Assert.Equal(4, groups[1].Segments.Count);

        Assert.Equal(Constants.Other, groups[2].PatternName);
        Assert.Equal(7, groups[2].Segments.Count);

    }

    [Fact]
    public void OtherWithMultipleIntervalsInBetween()
    {
        List<Segment> segments = new List<Segment> { ShortInterval, Switch, MedInterval, Switch, LongInterval };
        var mapalyzr = new Mapalyzr();
        var groups = mapalyzr.IdentifyPatterns(segments);

        Assert.Single(groups);
        Assert.Equal(Constants.Other, groups[0].PatternName);
        Assert.Equal(5, groups[0].Segments.Count);
    }

    [Fact]
    public void OtherInBetween()
    {
        var patterns = new List<Segment>
        {
            Switch,
            TwoStack,
            ZigZag,
            TwoStack,
            TwoStack,
            ThreeStack,
            Switch,
            ShortInterval,
            ShortInterval,
            Switch
        };

        var groups = new Mapalyzr().IdentifyPatterns(patterns);

        Assert.Equal(5, groups.Count);
        Assert.Equal(Constants.Other, groups[0].PatternName);
        Assert.Equal(4, groups[0].Segments.Count);
        Assert.Equal(Constants.VaryingStacks, groups[1].PatternName);
        Assert.Equal(3, groups[1].Segments.Count);
        Assert.Equal(Constants.Other, groups[2].PatternName);
        Assert.Equal(3, groups[2].Segments.Count);
        Assert.Equal(Constants.SlowStretch, groups[3].PatternName);
        Assert.Equal(2, groups[3].Segments.Count);
        Assert.Equal(Constants.Other, groups[4].PatternName);
        Assert.Equal(2, groups[4].Segments.Count);
    }

    [Fact]
    public void TestEvenCircle()
    {
        var patterns = new List<Segment>
        {
            Switch,
            TwoStack,
            Switch,
            TwoStack,
            Switch,
            TwoStack
        };

        var groups = new Mapalyzr().IdentifyPatterns(patterns);

        Assert.Single(groups);
        Assert.Equal(Constants.EvenCircles, groups[0].PatternName);
        Assert.Equal(6, groups[0].Segments.Count);
    }

    [Fact]
    public void TestEvenCircleWithIntervalsStartAndEnd()
    {
        var patterns = new List<Segment>
        {
            new Segment(Constants.LongInterval, new List<Note> { CreateNote(0, 0), CreateNote(0, 10) }),
            new Segment(Constants.Switch, new List<Note> { CreateNote(0, 10), CreateNote(1, 10.1) }),
            new Segment(Constants.TwoStack, new List<Note> { CreateNote(1, 10.1), CreateNote(1, 10.2) }),
            new Segment(Constants.Switch, new List<Note> { CreateNote(1, 10.2), CreateNote(0, 10.3) }),
            new Segment(Constants.TwoStack, new List<Note> { CreateNote(0, 10.3), CreateNote(0, 10.4) }),
            new Segment(Constants.Switch, new List<Note> { CreateNote(0, 10.4), CreateNote(1, 10.5) }),
            new Segment(Constants.TwoStack, new List<Note> { CreateNote(1, 10.5), CreateNote(1, 10.6) }),
            new Segment(Constants.MedInterval, new List<Note> { CreateNote(1, 10.6), CreateNote(0, 12.6) })
        };

        var groups = new Mapalyzr().IdentifyPatterns(patterns);

        Assert.Single(groups);
        Assert.Equal(Constants.EvenCircles, groups[0].PatternName);
        Assert.Equal(8, groups[0].Segments.Count);
    }


}