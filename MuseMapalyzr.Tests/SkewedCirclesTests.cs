namespace MuseMapalyzr.Tests;
using MuseMapalyzr;
public class SkewedCirclesTests
{

    Note ValidNote1 = new Note(0, 0);
    Note ValidNote2 = new Note(0, 0.1 * Constants.DefaultSampleRate);
    Note ValidNote3 = new Note(0, 0.2 * Constants.DefaultSampleRate);
    Note ValidNote4 = new Note(0, 0.3 * Constants.DefaultSampleRate);
    Note ValidNote5 = new Note(0, 0.4 * Constants.DefaultSampleRate);

    [Fact]
    public void TestValidCurrentFourStackWhenFirst()
    {
        var currentPattern = new Segment(Constants.FourStack, new List<Note>(), 0, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidCurrentZigZagWhenFirst()
    {
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { ValidNote1, ValidNote2, ValidNote3 }, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenFirst()
    {
        var currentPattern = new Segment(Constants.SingleStreams, new List<Note>(), 0, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentZigZagWhenTwoStackPrevious()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote1, ValidNote2 }, 0);
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { ValidNote2, ValidNote3, ValidNote4 }, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentZigZagWhenTwoStackPrevious()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote1, ValidNote2 }, 0);
        var currentPattern = new Segment(Constants.ZigZag, new List<Note> { ValidNote2, ValidNote3, ValidNote4, ValidNote5 }, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentThreeStackWhenZigZagPrevious()
    {
        var previousPattern = new Segment(Constants.ZigZag, new List<Note> { ValidNote1, ValidNote2, ValidNote3 }, 0);
        var currentPattern = new Segment(Constants.ThreeStack, new List<Note> { ValidNote3, ValidNote4, ValidNote5 }, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenValidPrevious()
    {
        var previousPattern = new Segment(Constants.ZigZag, new List<Note> { ValidNote1, ValidNote2, ValidNote3 }, 0);
        var currentPattern = new Segment(Constants.Switch, new List<Note> { ValidNote3, ValidNote4 }, 0);

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentPatternWithDifferentTimeDifferenceNotAdded()
    {
        var previousPattern = new Segment(Constants.Switch, new List<Note> { ValidNote1, ValidNote2 });
        var currentPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote2, ValidNote4 });

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestInvalidWhenGapBetweenSwitchAndStack()
    {
        var previousPattern = new Segment(Constants.Switch, new List<Note> { ValidNote1, ValidNote2 });
        var currentPattern = new Segment(Constants.ThreeStack, new List<Note> { ValidNote3, ValidNote4, ValidNote5 });

        var group = new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }


}