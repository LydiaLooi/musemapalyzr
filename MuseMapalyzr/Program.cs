
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MuseMapalyzr
{
    public class InputData
    {
        public void CalculateAndExportAllDifficulties(string dataDir)
        {
            string[] allFiles = Directory.GetFiles(dataDir);

            ProcessDifficulties(allFiles);
        }

        public void CalculateAndExportFilteredDifficulties(string filterBy, string dataDir)
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

            ProcessDifficulties(filtered.ToArray());

        }

        public void ProcessDifficulties(string[] files)
        {

            DateTime now = DateTime.Now;
            DifficultyCalculation difficultyCalculation = new();
            string outputFile = Path.Combine(Constants.OutputDir, $"{now.ToString("yyyy-MM-dd_HH-mm-ss")}_difficulties_data.txt");

            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                foreach (string filename in files)
                {
                    // try
                    // {
                    string charSeparator = "\\";
                    MuseSwiprMap mMap = MuseSwiprMap.FromKoreographAsset(filename);
                    string name = filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0];

                    Console.WriteLine($"Analysing : {name} Samplerate: {mMap.SampleRate}");
                    CustomLogger.Instance.Info($"Analysing : {name} Samplerate: {mMap.SampleRate}");
                    CustomLogger.Instance.PatternLog($"Analysing : {name} Samplerate: {mMap.SampleRate}");

                    using (StreamWriter outfile = new StreamWriter($"{Constants.AnalysisDir}/{name}", false, Encoding.UTF8))
                    {
                        DifficultyCalculation.WeightingResults weightResults = difficultyCalculation.CalculateDifficulty(mMap.Notes, outfile, mMap.SampleRate);
                        writer.WriteLine($"{filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0]}||{weightResults.RankedWeightedDifficulty:F2}||{weightResults.RankedWeighting:F2}||{weightResults.RankedDifficulty:F2}||{weightResults.UnrankedWeightedDifficulty:F2}||{weightResults.UnrankedWeighting:F2}||{weightResults.UnrankedDifficulty:F2}");
                    }
                    // }
                    // catch (Exception e)
                    // {
                    //     // Log the error
                    //     // // Console.WriteLine($"ERROR parsing a file: {e.Message}");
                    //     throw e;
                    //     // continue;
                    // }
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
                List<double> values = new List<double> {
                    1,1,1,1,1,1,1,9,9,9
                    };
                double results = DifficultyCalculation.WeightedAverageOfValues(values, 0.3, 1, 0.1);
                double results2 = DifficultyCalculation.WeightedAverageOfValues(values, 0.2, 1, 0.1);
                Console.WriteLine($"Weighted: {results} Weighted: {results2} Mean: {values.Average()}");
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