namespace MuseMapalyzr.Tests;
using MuseMapalyzr;

public class IntervalBetweenPatternsIsTolerable
{

    private Note CreateNote(int lane, double seconds)
    {
        return new Note(lane, seconds * Constants.DefaultSampleRate);
    }

    [Fact]
    public void TestInvalidToleranceWithGap()
    {
        var previousPattern = new Segment("", new List<Note> { CreateNote(0, 1), CreateNote(0, 2) });
        var currentPattern = new Segment("", new List<Note> { CreateNote(0, 3), CreateNote(0, 4) });

        var valid = new OtherPattern("", new List<Segment>()).IntervalBetweenSegmentsIsTolerable(previousPattern, currentPattern);
        Assert.False(valid);
    }

    [Fact]
    public void TestValidToleranceWithNoGap()
    {
        var previousPattern = new Segment("", new List<Note> { CreateNote(0, 21.60), CreateNote(0, 21.78) });
        var currentPattern = new Segment("", new List<Note> { CreateNote(0, 21.78), CreateNote(0, 21.96) });

        var valid = new OtherPattern("", new List<Segment>()).IntervalBetweenSegmentsIsTolerable(previousPattern, currentPattern);
        Assert.True(valid);
    }

    [Fact]
    public void TestInvalidTolerance()
    {
        var previousPattern = new Segment("", new List<Note> { CreateNote(0, 1), CreateNote(0, 2) });
        var currentPattern = new Segment("", new List<Note> { CreateNote(0, 4), CreateNote(0, 5) });

        var valid = new OtherPattern("", new List<Segment>()).IntervalBetweenSegmentsIsTolerable(previousPattern, currentPattern);
        Assert.False(valid);
    }


}