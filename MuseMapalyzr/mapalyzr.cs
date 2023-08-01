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

        private void ResetGroups()
        {
            this.Groups = new List<Pattern>
    {
        new EvenCirclesGroup(Constants.EvenCircles, new List<Segment>()),
        new SkewedCirclesGroup(Constants.SkewedCircles, new List<Segment>()),
        new VaryingStacksPattern(Constants.VaryingStacks, new List<Segment>()),
        new NothingButTheoryGroup(Constants.NothingButTheory, new List<Segment>()),
        new SlowStretchPattern(Constants.SlowStretch, new List<Segment>())
    };
            this.OtherPattern = new OtherPattern(Constants.Other, new List<Segment>());
        }

        public List<Pattern> IdentifyPatterns(List<Segment> segmentsList, bool mergeMergable = true)
        {
            for (int i = 0; i < segmentsList.Count; i++)
            {
                var currentSegment = segmentsList[i];

                Segment? previousSegment = null;
                if (segmentsList.Count > 1 && i != 0)
                {
                    previousSegment = segmentsList[i - 1];
                }

                HandleEachGroup(previousSegment, currentSegment);
                if (!Reset)
                {
                    OtherPattern.CheckSegment(currentSegment);
                }

                // We have gone through all the defined groups...
                if (!Added)
                {
                    // Append OtherGroup if no other groups were appendable
                    if (OtherPattern.Segments.Count > 0)
                    {
                        Patterns.Add(new OtherPattern(Constants.Other, OtherPattern.Segments, OtherPattern.StartSample, OtherPattern.EndSample));
                    }
                    OtherPattern.ResetGroup(previousSegment, currentSegment);  // reset OtherGroup

                    // Reset all groups with current pattern.
                    foreach (Pattern group in Groups)
                    {
                        group.ResetGroup(previousSegment, currentSegment);
                    }
                }
            }

            HandleLastPatterns(mergeMergable);

            return ReturnFinalPatterns(mergeMergable);
        }

        private void HandleEachGroup(Segment? previousSegment, Segment currentSegment)
        {
            Added = false;  // has this pattern been added?
            Reset = false;  // have we done a reset?
            foreach (Pattern group in Groups)
            {
                bool added = group.CheckSegment(currentSegment).Value;
                if (added)
                {
                    Added = true;
                }
                else
                {
                    // Only set it to inactive if it's already begun adding stuff
                    if (group.Segments.Count > 0)
                    {
                        group.IsActive = false;
                    }

                    // Check if the group is appendable
                    if (group.IsAppendable())
                    {
                        Added = true;
                        HandleAppendableGroup(
                            group: group,
                            previousSegment: previousSegment,
                            currentSegment: currentSegment
                        );
                        Reset = true;
                        return;  // STOP LOOKING !! WE FOUND SOMETHING
                    }
                }
            }
        }

        private List<Pattern>? HandleLastPatterns(bool mergeMergable = true)
        {
            // Do last check
            foreach (Pattern lastCheckPattern in Groups)
            {
                if (lastCheckPattern.IsAppendable())
                {
                    Pattern lastPatternCopy = lastCheckPattern.CreateCopy();
                    Patterns.Add(lastPatternCopy);
                    return ReturnFinalPatterns(mergeMergable);
                }
            }
            if (OtherPattern.Segments.Count > 0)
            {
                // If there is a hanging SINGLE Interval at the end of the pattern, don't add it... unless it is the only one in the group list
                if (Patterns.Count == 0 || !(OtherPattern.Segments.Count == 1 && SegmentIsInterval(OtherPattern.Segments[0])))
                {
                    Pattern lastPatternCopy = new OtherPattern(
                        Constants.Other,
                        new List<Segment>(OtherPattern.Segments),
                        OtherPattern.StartSample,
                        OtherPattern.EndSample
                    );
                    Patterns.Add(lastPatternCopy);
                }
            }
            return null;
        }

        public List<Pattern> ReturnFinalPatterns(bool mergeMergable = true)
        {
            // Console.WriteLine($"_return_final_patterns : {mergeMergable}");
            Console.WriteLine($"Patterns ({Patterns.Count}): {string.Join(", ", Patterns.Select(p => $"{p.PatternName} ({p.Segments.Count})"))}");

            if (!mergeMergable)
            {
                return Patterns;
            }

            List<Pattern> newGroups = new List<Pattern>();
            Pattern currentMergable = null;
            bool isFirst = true;
            foreach (Pattern pg in Patterns)
            {
                if (pg.PatternName != Constants.Other && pg.PatternName != Constants.SlowStretch)
                {
                    currentMergable = HandleNonMergableGroup(newGroups, currentMergable, pg);
                    isFirst = true;
                }
                else
                {
                    (currentMergable, isFirst) = HandleMergableGroup(newGroups, currentMergable, pg, isFirst);
                }
            }

            if (currentMergable != null && currentMergable.Segments.Count > 0)
            {
                newGroups.Add(currentMergable);
            }

            Console.WriteLine($"Merged patterns ({newGroups.Count}): {string.Join(", ", newGroups.Select(p => $"{p.PatternName} ({p.Segments.Count})"))}");
            return newGroups;
        }

        private void HandleAppendableGroup(Pattern group, Segment previousSegment, Segment currentSegment)
        {
            // Need to first check if OtherGroup has stragglers...
            if (group.Segments.Count < OtherPattern.Segments.Count)
            {
                // THERE ARE STRAGGLERS.
                OtherPattern otherGroup = new OtherPattern(
                    Constants.Other,
                    OtherPattern.Segments.GetRange(0, OtherPattern.Segments.Count - group.Segments.Count) // If things go wrong, this would be something to look at
                );
                Patterns.Add(otherGroup);
            }
            Pattern groupCopy = group.CreateCopy();
            Patterns.Add(groupCopy);
            // Reset all groups with current pattern.
            foreach (Pattern group2 in Groups)
            {
                group2.ResetGroup(previousSegment, currentSegment);
            }
            OtherPattern.ResetGroup(previousSegment, currentSegment);
        }

        private Pattern HandleNonMergableGroup(List<Pattern> newPatternsList, Pattern currentMergable, Pattern pg)
        {
            if (currentMergable != null)
            {
                if (currentMergable.PatternName == Constants.Other)
                {
                    if (!SegmentIsInterval(currentMergable.Segments.Last()))
                    {
                        currentMergable.Segments = currentMergable.Segments.Take(currentMergable.Segments.Count - 1).ToList();
                    }
                    newPatternsList.Add(currentMergable);
                }
                else if (currentMergable.PatternName == Constants.SlowStretch)
                {
                    newPatternsList.Add(currentMergable);
                }
                else
                {
                    throw new ArgumentException($"HandleNonMergableGroup Unsupported mergable pattern of: {currentMergable.PatternName}");
                }
                currentMergable = null;
            }
            newPatternsList.Add(pg);
            return currentMergable;
        }

        private (Pattern, bool) HandleMergableGroup(List<Pattern> newGroups, Pattern currentMergable, Pattern pattern, bool isFirst)
        {
            if (currentMergable == null)
            {
                currentMergable = GetEmptyMergablePattern(pattern);
            }

            if (currentMergable.PatternName != pattern.PatternName)
            {
                if (pattern.PatternName == Constants.Other && pattern.Segments.Count == 1)
                {
                    return (currentMergable, isFirst);
                }
                newGroups.Add(currentMergable);
                isFirst = true;
                if (pattern.PatternName == Constants.Other || pattern.PatternName == Constants.SlowStretch)
                {
                    currentMergable = GetEmptyMergablePattern(pattern);
                }
                else
                {
                    currentMergable = null;
                }
            }
            if (isFirst)
            {
                currentMergable = HandleFirstMergableGroup(currentMergable, pattern);
            }
            else
            {
                currentMergable = HandleNotFirstMergableGroup(currentMergable, pattern);
            }
            if (currentMergable != null)
            {
                isFirst = false;
            }
            return (currentMergable, isFirst);
        }

        private Pattern GetEmptyMergablePattern(Pattern pattern)
        {
            if (pattern.PatternName == Constants.Other)
            {
                return new OtherPattern(Constants.Other, new List<Segment>());
            }
            else if (pattern.PatternName == Constants.SlowStretch)
            {
                return new SlowStretchPattern(Constants.SlowStretch, new List<Segment>());
            }
            else
            {
                throw new ArgumentException($"GetEmptyMergablePattern Unsupported mergable pattern of: {pattern.PatternName}");
            }
        }

        private Pattern HandleFirstMergableGroup(Pattern currentMergable, Pattern pattern)
        {
            if (this.Patterns.Count > 1 && pattern.Segments.Count == 1 && SegmentIsInterval(pattern.Segments[0]))
            {
                currentMergable = null;
            }
            else
            {
                currentMergable.Segments.AddRange(pattern.Segments);
            }
            return currentMergable;
        }

        private Pattern HandleNotFirstMergableGroup(Pattern currentMergable, Pattern pattern)
        {
            if (currentMergable.PatternName == Constants.Other)
            {
                if (pattern.Segments.Count > 2)
                {
                    if (SegmentIsInterval(pattern.Segments[0]))
                    {
                        currentMergable.Segments.AddRange(pattern.Segments.GetRange(1, pattern.Segments.Count - 1));
                    }
                    else
                    {
                        currentMergable.Segments.AddRange(pattern.Segments.GetRange(2, pattern.Segments.Count - 2));
                    }
                }
            }
            else if (currentMergable.PatternName == Constants.SlowStretch)
            {
                currentMergable.Segments.AddRange(pattern.Segments.GetRange(1, pattern.Segments.Count - 1));
            }
            else
            {
                throw new ArgumentException($"HandleNotFirstMergableGroup Unsupported mergable pattern: {currentMergable.PatternName}");
            }
            return currentMergable;
}


    }
}