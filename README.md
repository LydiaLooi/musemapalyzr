
# Usage

`cd MuseMapalyzr`

- Process all difficulties in `data`: `dotnet run`
- Process filtered difficulties: `dotnet run search string` e.g., `dotnet run speculation`
- See the multiplier graph: `dotnet run graph`


# TODO:
## To be continued
- Dark omen 7 NPS cap is like ~16 but then 7000 (which means it's not doing any cutting of notes) is like ~15?????
- Is something wrong with the single stream density nerfing thing???
- We set ranked NPS cap to 8 and unranked to 13 and that's when we discovered that dark omen was just being weird. 
- Could it be a weird interaction with the density stuff? can't be though??? riught??


## Still to do:
- Ranked incorporates the "normal length" song threshold when determining the density.


# Packages and stuff:

For the graphing of multipliers:

`cd MuseMapalyzr`
`dotnet add package OxyPlot.WindowsForms --version 2.1.2`

