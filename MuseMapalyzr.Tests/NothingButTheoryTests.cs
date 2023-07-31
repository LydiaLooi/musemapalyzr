namespace MuseMapalyzr.Tests;
using MuseMapalyzr;
public class NothingButTheoryTests
{
    private Note validNote1 = new Note(0, 0);
    private Note validNote2 = new Note(0, 0.1 * Constants.DefaultSampleRate);
    private Note validNote3 = new Note(0, 0.2 * Constants.DefaultSampleRate);
    private Note validNote4 = new Note(0, 0.3 * Constants.DefaultSampleRate);
    private Note validNote5 = new Note(0, 0.4 * Constants.DefaultSampleRate);
    private Note validNote6 = new Note(0, 0.5 * Constants.DefaultSampleRate);
    private Note validNote7 = new Note(0, 0.6 * Constants.DefaultSampleRate);
    private Note validNote8 = new Note(0, 0.7 * Constants.DefaultSampleRate);


    [Fact]
    public void TestValidCurrentTwoStackWhenFirst()
    {
        var currentPattern = new Segment(Constants.TwoStack, new List<Note>(), 0, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidCurrentZigZagWhenFirst()
    {
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { validNote1, validNote2, validNote3, validNote4 }, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenFirst()
    {
        var currentPattern = new Segment(Constants.SingleStreams, new List<Note>(), 0, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentZigZagWhenTwoStackPrevious()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { validNote1, validNote2 }, 0);
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { validNote2, validNote3, validNote4, validNote5 }, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentZigZagWhenTwoStackPrevious()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { validNote1, validNote2 }, 0);
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { validNote2, validNote3, validNote4 }, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentTwoStackWhenZigZagPrevious()
    {
        var previousPattern = new Segment(Constants.ZigZag, new List<Note> { validNote1, validNote2, validNote3, validNote4 }, 0);
        var currentPattern = new Segment(Constants.TwoStack, new List<Note> { validNote4, validNote5 }, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenValidPrevious()
    {
        var previousPattern = new Segment(Constants.ZigZag, new List<Note> { validNote1, validNote2, validNote3, validNote4 }, 0);
        var currentPattern = new Segment(Constants.Switch, new List<Note> { validNote4, validNote5 }, 0);

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentPatternWithDifferentTimeDifferenceNotAdded()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { validNote1, validNote3 });
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { validNote3, validNote4, validNote5, validNote6 });

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestInvalidWhenGapBetweenSwitchAndStack()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { validNote1, validNote2 });
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { validNote3, validNote4, validNote5, validNote6 });

        var group = new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }


}