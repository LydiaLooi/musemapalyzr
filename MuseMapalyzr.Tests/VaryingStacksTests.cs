namespace MuseMapalyzr.Tests;
using MuseMapalyzr;

public class VaryingStacksTests
{
    Note ValidNote1 = new Note(0, 0);
    Note ValidNote2 = new Note(0, 0.1 * Constants.DefaultSampleRate);
    Note ValidNote3 = new Note(0, 0.2 * Constants.DefaultSampleRate);
    Note ValidNote4 = new Note(0, 0.3 * Constants.DefaultSampleRate);
    Note ValidNote5 = new Note(0, 0.4 * Constants.DefaultSampleRate);
    Note ValidNote6 = new Note(0, 0.5 * Constants.DefaultSampleRate);
    Note ValidNote7 = new Note(0, 0.6 * Constants.DefaultSampleRate);
    Note ValidNote8 = new Note(0, 0.7 * Constants.DefaultSampleRate);
    Note ValidNote9 = new Note(0, 0.8 * Constants.DefaultSampleRate);

    [Fact]
    public void TestValidCurrentTwoStackWhenFirst()
    {
        var currentPattern = new Segment(Constants.TwoStack, new List<Note>(), 0, 0);

        var group = new VaryingStacksPattern(Constants.VaryingStacks, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidIntervalWhenFirst()
    {
        var currentPattern = new Segment(Constants.ShortInterval, new List<Note>(), 0, 0);

        var group = new VaryingStacksPattern(Constants.VaryingStacks, new List<Segment>());
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidStackWhenIntervalPrevious()
    {
        var previousPattern = new Segment(Constants.ShortInterval, new List<Note>(), 0, 0);
        var currentPattern = new Segment(Constants.TwoStack, new List<Note>(), 0, 0);

        var group = new VaryingStacksPattern(Constants.VaryingStacks, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidStackWhenStackPrevious()
    {
        var previousPattern = new Segment(Constants.TwoStack, new List<Note>(), 0, 0);
        var currentPattern = new Segment(Constants.FourStack, new List<Note>(), 0, 0);

        var group = new VaryingStacksPattern(Constants.VaryingStacks, new List<Segment> { previousPattern });
        var added = group.CheckSegment(currentPattern);
        Assert.True(added);
    }

    [Fact]
    public void TestValidStackWhenEndsWithIntervalReturnsFalseButAdds()
    {
        var currentPattern = new Segment(Constants.LongInterval, new List<Note>(), 0, 0);

        var patterns = new List<Segment>
    {
        new Segment(Constants.ShortInterval, new List<Note>(), 0, 0),
        new Segment(Constants.TwoStack, new List<Note>(), 0, 0),
        new Segment(Constants.ThreeStack, new List<Note>(), 0, 0),
    };

        var group = new VaryingStacksPattern(Constants.VaryingStacks, patterns);
        var added = group.CheckSegment(currentPattern);
        Assert.False(added);
        Assert.Equal(Constants.LongInterval, group.Segments[^1].SegmentName);
    }


}