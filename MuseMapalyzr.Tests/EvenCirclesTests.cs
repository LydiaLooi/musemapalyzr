namespace MuseMapalyzr.Tests;
using MuseMapalyzr;
public class EvenCirclesTests
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
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidCurrentSwitchWhenFirst()
    {
        var currentPattern = new Segment(Constants.Switch, new List<Note>(), 0, 0);
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenFirst()
    {
        var currentPattern = new Segment(Constants.ZigZag, new List<Note>(), 0, 0);
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentSwitchWhenTwoStackPrevious()
    {
        var currentPattern = new Segment(Constants.Switch, new List<Note> { ValidNote2, ValidNote3 }, 0);
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote1, ValidNote2 }, 0);
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidCurrentThreeStackWhenSwitchPrevious()
    {
        var currentPattern = new Segment(Constants.ThreeStack, new List<Note> { ValidNote2, ValidNote3, ValidNote4 }, 0);
        var previousPattern = new Segment(Constants.Switch, new List<Note> { ValidNote1, ValidNote2 }, 0);
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestInvalidCurrentPatternWhenValidPrevious()
    {
        var currentPattern = new Segment(Constants.ZigZag, new List<Note>(), 0);
        var previousPattern = new Segment(Constants.Switch, new List<Note>(), 0);
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestValidCurrentPatternWithDifferentTimeDifferenceNotAdded()
    {
        var currentPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote2, ValidNote4 });
        var previousPattern = new Segment(Constants.Switch, new List<Note> { ValidNote1, ValidNote2 });
        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestInvalidWhenGapBetweenSwitchAndStack()
    {
        var previousPattern = new Segment(Constants.Switch, new List<Note> { ValidNote1, ValidNote2 });
        var currentPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote3, ValidNote4 });

        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
    }

    [Fact]
    public void TestStartingWithIntervalAddsItAndReturnsTrue()
    {
        var currentPattern = new Segment(Constants.ShortInterval, new List<Note> { ValidNote1, ValidNote2 });

        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
        Assert.Equal(Constants.ShortInterval, group.Segments[0].SegmentName);
    }

    [Fact]
    public void TestEndingWithIntervalAddsItAndReturnsFalse()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note> { ValidNote1, ValidNote2 });
        var currentPattern = new Segment(Constants.ShortInterval, new List<Note> { ValidNote2, ValidNote3 });

        var group = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
        Assert.Equal(Constants.ShortInterval, group.Segments.Last().SegmentName);
    }


}