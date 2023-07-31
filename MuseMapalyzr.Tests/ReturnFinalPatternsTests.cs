namespace MuseMapalyzr.Tests;
using MuseMapalyzr;

public class ReturnFinalPatternsTests
{
    [Fact]
    public void TestMergesOthersTogether()
    {
        var mp = new Mapalyzr();

        // B, C overlapped. C, D overlapped.
        var o2 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o3 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("C", new List<Note>()), new Segment("D", new List<Note>()), new Segment("E", new List<Note>()) });
        var o1 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()), new Segment("C", new List<Note>()) });
        mp.Patterns = new List<Pattern> { o1, o2, o3 };
        var results = mp.ReturnFinalPatterns();
        Assert.Single(results);
        Assert.Equal(Constants.Other, results[0].PatternName);
        Assert.Equal(5, results[0].Segments.Count);
    }

    [Fact]
    public void TestDoesNotMergeWithPatternsBetween()
    {
        var mp = new Mapalyzr();

        // B, C overlapped. C, D overlapped.
        var o1 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()), new Segment("C", new List<Note>()) });
        var o2 = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o3 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("C", new List<Note>()), new Segment("D", new List<Note>()), new Segment("E", new List<Note>()) });
        mp.Patterns = new List<Pattern> { o1, o2, o3 };
        var results = mp.ReturnFinalPatterns();
        Assert.Equal(3, results.Count);
        Assert.Equal(Constants.Other, results[0].PatternName);
        Assert.Equal(Constants.EvenCircles, results[1].PatternName);
        Assert.Equal(Constants.Other, results[2].PatternName);
        Assert.Equal(2, results[0].Segments.Count);  // A, B
        Assert.Equal(3, results[1].Segments.Count);  // B, C, D
        Assert.Equal(3, results[2].Segments.Count);  // C, D, E
        Assert.Equal("C", results[2].Segments[0].SegmentName);
        Assert.Equal("D", results[2].Segments[1].SegmentName);
        Assert.Equal("E", results[2].Segments[2].SegmentName);
    }

    [Fact]
    public void TestMergesWithPatternsEitherSide()
    {
        var mp = new Mapalyzr();

        // B, C overlapped. C, D overlapped. E overlapped.
        var o1 = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()), new Segment("C", new List<Note>()) });
        var o2 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o3 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("C", new List<Note>()), new Segment("D", new List<Note>()), new Segment("E", new List<Note>()) });
        var o4 = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { new Segment("E", new List<Note>()), new Segment("F", new List<Note>()), new Segment("G", new List<Note>()) });
        mp.Patterns = new List<Pattern> { o1, o2, o3, o4 };
        var results = mp.ReturnFinalPatterns();
        Assert.Equal(3, results.Count);
        Assert.Equal(Constants.EvenCircles, results[0].PatternName);
        Assert.Equal(Constants.Other, results[1].PatternName);
        Assert.Equal(Constants.EvenCircles, results[2].PatternName);
        Assert.Equal(3, results[0].Segments.Count);  // A, B, C
        Assert.Equal(3, results[1].Segments.Count);  // B, C, D
        Assert.Equal("B", results[1].Segments[0].SegmentName);
        Assert.Equal("C", results[1].Segments[1].SegmentName);
        Assert.Equal("D", results[1].Segments[2].SegmentName);
        Assert.Equal(3, results[2].Segments.Count);  // E, F, G
    }

    [Fact]
    public void TestMergesSlowStretches()
    {
        var mp = new Mapalyzr();

        var o1 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()) });
        var o2 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o3 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("D", new List<Note>()), new Segment("E", new List<Note>()) });
        mp.Patterns = new List<Pattern> { o1, o2, o3 };
        var results = mp.ReturnFinalPatterns();
        Assert.Single(results);
        Assert.Equal(Constants.SlowStretch, results[0].PatternName);
        Assert.Equal(5, results[0].Segments.Count);  // A, B, C, D, E
    }

    [Fact]
    public void TestMergesSlowStretchesAndOthers()
    {
        var mp = new Mapalyzr();

        var o1 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()) });
        var o2 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o3 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("D", new List<Note>()), new Segment("E", new List<Note>()), new Segment("F", new List<Note>()) });
        var o4 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("E", new List<Note>()), new Segment("F", new List<Note>()), new Segment("G", new List<Note>()) });
        mp.Patterns = new List<Pattern> { o1, o2, o3, o4 };
        var results = mp.ReturnFinalPatterns();
        Assert.Equal(2, results.Count);
        Assert.Equal(Constants.SlowStretch, results[0].PatternName);
        Assert.Equal(4, results[0].Segments.Count);  // A, B, C, D
        Assert.Equal(Constants.Other, results[1].PatternName);
        Assert.Equal("D", results[1].Segments[0].SegmentName);
        Assert.Equal("G", results[1].Segments[^1].SegmentName);
    }

    [Fact]
    public void TestMergesSlowStretchesAndIgnoresOthersWith1SegmentOnly()
    {
        // Weird race condition. See we luv lama for a real example of 1 Other between slow stretches
        var mp = new Mapalyzr();

        var o1 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()) });
        var o2 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("B", new List<Note>()) });
        var o3 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("B", new List<Note>()), new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o4 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("D", new List<Note>()) });
        var o5 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("D", new List<Note>()), new Segment("E", new List<Note>()), new Segment("F", new List<Note>()) });

        mp.Patterns = new List<Pattern> { o1, o2, o3, o4, o5 };
        var results = mp.ReturnFinalPatterns();
        Assert.Single(results);
        Assert.Equal(Constants.SlowStretch, results[0].PatternName);
        Assert.Equal(6, results[0].Segments.Count);  // A, B, C, D, E, F
    }

    [Fact]
    public void TestMergesSlowStretchesWithManyPatterns()
    {
        var mp = new Mapalyzr();

        var o0 = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { new Segment("A", new List<Note>()), new Segment("B", new List<Note>()), new Segment("C", new List<Note>()) });

        var o1 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("C", new List<Note>()), new Segment("D", new List<Note>()) });
        var o2 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("D", new List<Note>()), new Segment("E", new List<Note>()), new Segment("F", new List<Note>()) });
        var o3 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("F", new List<Note>()), new Segment("G", new List<Note>()) });

        var o4 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("G", new List<Note>()), new Segment("H", new List<Note>()), new Segment("I", new List<Note>()) });
        var o5 = new OtherPattern(Constants.Other, new List<Segment> { new Segment("H", new List<Note>()), new Segment("I", new List<Note>()), new Segment("J", new List<Note>()) });

        var o6 = new EvenCirclesGroup(Constants.EvenCircles, new List<Segment> { new Segment("J", new List<Note>()), new Segment("K", new List<Note>()), new Segment("L", new List<Note>()) });

        var o7 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("L", new List<Note>()), new Segment("M", new List<Note>()) });
        var o8 = new SlowStretchPattern(Constants.SlowStretch, new List<Segment> { new Segment("M", new List<Note>()), new Segment("N", new List<Note>()) });

        mp.Patterns = new List<Pattern> { o0, o1, o2, o3, o4, o5, o6, o7, o8 };

        var results = mp.ReturnFinalPatterns();

        Assert.Equal(5, results.Count);
        Assert.Equal(Constants.EvenCircles, results[0].PatternName);
        Assert.Equal(Constants.SlowStretch, results[1].PatternName);
        Assert.Equal(Constants.Other, results[2].PatternName);
        Assert.Equal(Constants.EvenCircles, results[3].PatternName);
        Assert.Equal(Constants.SlowStretch, results[4].PatternName);

        Assert.Equal(5, results[1].Segments.Count);  // C, D, E, F, G
        Assert.Equal(3, results[2].Segments.Count);  // G, H, I
        Assert.Equal(3, results[4].Segments.Count);  // L, M, N
    }


}