
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


# TODO:
- Ranked incorporates the "normal length" song threshold when determining the density.
- Look at the pattern multipliers again.

# Packages and stuff:

For the graphing of multipliers:

`cd MuseMapalyzr`
`dotnet add package OxyPlot.WindowsForms --version 2.1.2`

