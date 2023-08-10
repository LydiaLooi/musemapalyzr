
# Usage

Make sure in `MuseMapalyzr` there are the following directories:
- `analysis`
- `difficulty_exports`
- `logs`

`cd MuseMapalyzr`

- Process all difficulties in `data`: `dotnet run`
- Process filtered difficulties: `dotnet run search string` e.g., `dotnet run speculation`
- See the multiplier graph: `dotnet run graph`

# Logging
- You can configure the logging level by changing it in `Logger.cs`
- `private static LogLevel DebugLevel = LogLevel.Debug;` <- Change to whatever you want.
- It will output to `logs.log` in `MuseMapalyzr/logs`


# Theory crafting and TO DO:
- Ranked incorporates the "normal length" song threshold when determining the density.
- Look at the pattern multipliers again.
    - Instead of doing an average of PATTERN multipliers (e.g., NBT, Other, Skewed Circles, ...), we will do an average of SEGMENT multipliers.
      - Since with pattern multipliers there was the problem where Other pattern could be like 100 segments long or something and that gets treated the same as a pattern with 4 segments in the average calculation
    - For segments that fall under a pattern (e.g., two stacks and switches in an Even circle pattern), we would calculate the pattern multiplier and then overwrite the segment multipliers with it.
    - For the Other pattern, we would just use each segment multipliers as is.
    - *** We will likely need to make it so PATTERN multipliers are less than individual segment multipliers
      - e.g., skewed circles currently capped at 1.75 should be less than the N-stacks which 4-stack is capped at 1.3x.
      - So some mix of moving the caps up for segment multipliers, and moving the caps down for pattern multipliers.
      - The THEORY is that if the segments don't fall into a pattern, then they're probably not in a predictable pattern and so more varied and harder.
      - OBJECTION!!! What about 2-stack switch 3-stack switch 4-stack switch 2-stack etc.....
        - This should be even circle...? Will need to check.
      - SO THE THEORY IS:
        - Patterns are things that are more "cheese-able" and that's why they are lower than the unpredictable segments.
        - In the future if we find more combinations of segments that are cheese-able, we could turn this into a recognisable pattern and weigh it lower.

## Predictions
- Erkfir: I think that "complex" maps with long stretches of patterns that would usually fall under "Other" like probably machine gun psystyle will g et buffed a fair bit in terms of the pattern multiplier.
- theory crafting session occurred ***
- Erkfir: after theory crafting: nvm i think we need to change some segment and pattern multipliers. 
- Kartsu: I agree



# Packages and stuff:

For the graphing of multipliers:

`cd MuseMapalyzr`
`dotnet add package OxyPlot.WindowsForms --version 2.1.2`

