
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
namespace MuseMapalyzr
{
    public class InputData
    {
        public void CalculateAndExportAllDifficulties(string dataDir)
        {
            string[] allFiles = Directory.GetFiles(dataDir);

            ProcessDifficulties(allFiles);
        }

        public void CalculateAndExportFilteredDifficulties(string filterBy, string dataDir, bool logData = false)
        {
            string[] allFiles = Directory.GetFiles(dataDir);

            List<string> filtered = new List<string>();
            // Get only files with the specified string in their names (case-insensitive)
            foreach (string file in allFiles)
            {
                if (file.ToLower().Contains(filterBy.ToLower()))
                {
                    filtered.Add(file);
                }
            }

            ProcessDifficulties(filtered.ToArray(), logData);

        }

        public void ProcessDifficulties(string[] files, bool logData = false)
        {

            DateTime now = DateTime.Now;
            DifficultyCalculation difficultyCalculation = new();
            string outputFile = Path.Combine(Constants.OutputDir, $"{now.ToString("yyyy-MM-dd_HH")}_difficulties_data.txt");
            string analysisFile = Path.Combine(Constants.OutputDir, $"{now.ToString("yyyy-MM-dd_HH")}_analysis.txt");

            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                using (StreamWriter analysisWriter = new StreamWriter(analysisFile, false, Encoding.UTF8))
                {
                    foreach (string filename in files)
                    {

                        string charSeparator = "\\";
                        MuseSwiprMap mMap = MuseSwiprMap.FromKoreographAsset(filename);
                        string name = filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0];

                        Console.WriteLine($"Analysing : {name} Samplerate: {mMap.SampleRate}");
                        CustomLogger.Instance.Info($"Analysing : {name} Samplerate: {mMap.SampleRate}");
                        CustomLogger.Instance.PatternLog($"Analysing : {name} Samplerate: {mMap.SampleRate}");

                        MapDetails mapDetails = difficultyCalculation.AnalyzeMap(mMap.Notes, mMap.SampleRate);

                        if (logData)
                        {


                            var formattedRankedDensities = mapDetails.RankedDensities.Select(d => d.ToString("F2").PadLeft(6));
                            var formattedPeakDensities = mapDetails.PeakDensities.Select(d => d.ToString("F2").PadLeft(6));
                            string rankedDensitiesStr = String.Join(", ", formattedRankedDensities);
                            string peakDensitiesStr = String.Join(", ", formattedPeakDensities);

                            var formattedHardestRankedDensities = mapDetails.RankedDensityDetails.HardestDensities.Select(d => d.ToString("F3").PadLeft(6));
                            var formattedHardestPeakDensities = mapDetails.PeakDensityDetails.HardestDensities.Select(d => d.ToString("F3").PadLeft(6));
                            string topRankedDensitiesStr = String.Join(", ", formattedHardestRankedDensities);
                            string topPeakDensitiesStr = String.Join(", ", formattedHardestPeakDensities);

                            var formattedRankedPatternMultiplierAverages = mapDetails.RankedDensityDetails.PatternAveragedMultipliers.Select(d => d.ToString("F2").PadLeft(6));
                            var formattePeakPatternMultiplierAverages = mapDetails.PeakDensityDetails.PatternAveragedMultipliers.Select(d => d.ToString("F2").PadLeft(6));
                            string rankPatternMultiplierAvgs = String.Join(", ", formattedRankedPatternMultiplierAverages);
                            string peakPatternMultiplierAvgs = String.Join(", ", formattePeakPatternMultiplierAverages);

                            analysisWriter.WriteLine($"Analysing : {name} Samplerate: {mMap.SampleRate}");
                            analysisWriter.WriteLine($"Num Notes: {mapDetails.Notes.Count,10}");
                            analysisWriter.WriteLine($"Ranked   : {mapDetails.RankedDifficulty,10:F2}");
                            analysisWriter.WriteLine($"Peak     : {mapDetails.PeakDifficulty,10:F2}");
                            analysisWriter.WriteLine();
                            analysisWriter.WriteLine($"Rank| Densities   : {rankedDensitiesStr}");
                            analysisWriter.WriteLine($"Rank| Multipliers : {rankPatternMultiplierAvgs}");
                            analysisWriter.WriteLine();
                            analysisWriter.WriteLine($"Peak| Densities   : {peakDensitiesStr}");
                            analysisWriter.WriteLine($"Peak| Multipliers : {peakPatternMultiplierAvgs}");
                            analysisWriter.WriteLine();
                            analysisWriter.WriteLine($"Rank| Top hardest Ranked Densities: {topRankedDensitiesStr}");
                            analysisWriter.WriteLine($"Peak| Top hardest Peak Densities  : {topPeakDensitiesStr}");
                            analysisWriter.WriteLine();

                            analysisWriter.WriteLine(String.Format("\n{0,6} {1,10} {2,10} {3,10} {4,10} {5,16} {6,10} {7,10}", "Type", "Ceiling", "Hardest", "Penalty", "Penalised", "Addit. Stars", "Cum. Sum", "Added Stars"));
                            string ranked = "Rank";
                            string peak = "Peak";
                            analysisWriter.WriteLine($"{ranked,6} {mapDetails.RankedDensityDetails.Ceiling,10:F3} {mapDetails.RankedDensityDetails.Hardest,10:F3} {mapDetails.RankedDensityDetails.Penalty,10:F3} {mapDetails.RankedDensityDetails.FinalPenalisedBase,10:F3} {mapDetails.RankedDensityDetails.AdditionalStars,16:F3} {mapDetails.RankedDensityDetails.CumulativeSum,10:F3} {mapDetails.RankedDensityDetails.AddedDifficulty,10:F3}          FinalPenalised + Added = {mapDetails.RankedDensityDetails.FinalPenalisedBase + mapDetails.RankedDensityDetails.AddedDifficulty:F2} * {Constants.BaseDifficultyMultiplier} = {(mapDetails.RankedDensityDetails.FinalPenalisedBase + mapDetails.RankedDensityDetails.AddedDifficulty) * Constants.BaseDifficultyMultiplier:F2}");
                            analysisWriter.WriteLine($"{peak,6} {mapDetails.PeakDensityDetails.Ceiling,10:F3} {mapDetails.PeakDensityDetails.Hardest,10:F3} {mapDetails.PeakDensityDetails.Penalty,10:F3} {mapDetails.PeakDensityDetails.FinalPenalisedBase,10:F3} {mapDetails.PeakDensityDetails.AdditionalStars,16:F3} {mapDetails.PeakDensityDetails.CumulativeSum,10:F3} {mapDetails.PeakDensityDetails.AddedDifficulty,10:F3}          FinalPenalised + Added = {mapDetails.PeakDensityDetails.FinalPenalisedBase + mapDetails.PeakDensityDetails.AddedDifficulty:F2} * {Constants.BaseDifficultyMultiplier} = {(mapDetails.PeakDensityDetails.FinalPenalisedBase + mapDetails.PeakDensityDetails.AddedDifficulty) * Constants.BaseDifficultyMultiplier:F2}");
                            analysisWriter.WriteLine();
                            analysisWriter.WriteLine("Pattern Type Counts:");
                            foreach (KeyValuePair<string, int> kvp in mapDetails.SimplePatternData)
                            {
                                // Access the key and value
                                string key = kvp.Key;
                                int value = kvp.Value;

                                // Do something with the key and value
                                analysisWriter.WriteLine($"{key,20} {value,2}");
                            }

                            analysisWriter.WriteLine("\nSegment Type Counts:");
                            analysisWriter.WriteLine(String.Format("{0, 20} {1, 15} {2, 15} {3, 15} {4, 15} {5, 15} {6, 15}", "Name", "Seg. Count", "Note Count", "% of map", "Min NPS", "Avg NPS", "Max NPS"));

                            foreach (KeyValuePair<string, MapDetails.SegmentData> kvp in mapDetails.MapSegmentData)
                            {
                                // Access the key and value
                                string key = kvp.Key;
                                MapDetails.SegmentData value = kvp.Value;

                                // Do something with the key and value
                                analysisWriter.WriteLine($"{key,20} {value.Count,15} {value.TotalNotes,15} {value.Proportion * 100,15:F2} {value.MinNPS,15:F2} {value.AvgNPS,15:F2} {value.MaxNPS,15:F2}");
                            }


                            analysisWriter.WriteLine("\n\n");

                        }


                        writer.WriteLine($"{filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0]}||{mapDetails.RankedDifficulty:F2}||-1||-1||{mapDetails.PeakDifficulty:F2}||-1||-1");
                    }
                }
            }
        }
    }

    class MainProgram
    {
        static void Main(string[] args)
        {
            InputData inputData = new();

            CustomLogger.Instance.Info("Starting Application");

            if (args.Length == 0)
            {
                // If no arguments are provided, run export all difficulties
                inputData.CalculateAndExportAllDifficulties(Constants.DataDir);
            }
            else if (args[0] == "graph")
            {
                if (args.Length > 1 && args[1] == "unranked")
                {
                    Console.WriteLine("Graphing unranked");
                    MultiplierGraphing.Graph(false);
                }
                else
                {
                    MultiplierGraphing.Graph(true);
                }

            }
            else if (args[0] == "test")
            {
                for (double i = 0; i < 50; i++)
                {
                    double result = Utils.ScaleDifficulty(i);
                    Console.WriteLine($"{i} -> {result}");
                }
            }
            else if (args[0] == "debug" && args.Count() > 1)
            {
                List<String> newArgs = args.ToList();
                newArgs.RemoveAt(0);
                string filterBy = string.Join(" ", newArgs);
                inputData.CalculateAndExportFilteredDifficulties(filterBy, Constants.DataDir, true);
            }
            else
            {
                // If any argument is provided, run filtered difficulties
                string filterBy = string.Join(" ", args);
                inputData.CalculateAndExportFilteredDifficulties(filterBy, Constants.DataDir);
            }
            CustomLogger.Instance.Close(); // Close the logger at the end
        }
    }
}