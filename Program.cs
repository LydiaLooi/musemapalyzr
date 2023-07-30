
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
            string outputFile = Path.Combine(Constants.OUTPUT_DIR, $"{now.ToString("yyyy-MM-dd_HH-mm-ss")}_difficulties_data.txt");

            using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
            {
                foreach (string filename in files)
                {
                    try
                    {
                        string charSeparator = "\\";
                        MuseSwiprMap mMap = MuseSwiprMap.FromKoreographAsset(filename);
                        string name = filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0];

                        using (StreamWriter outfile = new StreamWriter($"analysis/{name}", false, Encoding.UTF8))
                        {
                            DifficultyCalculation.WeightingResults weightResults = difficultyCalculation.CalculateDifficulty(mMap.Notes, outfile, mMap.SampleRate);
                            writer.WriteLine($"{filename.Split(new string[] { charSeparator }, StringSplitOptions.None)[^1].Split(".asset")[0]}||{weightResults.WeightedDifficulty:F2}||{weightResults.Weighting:F2}||{weightResults.Difficulty:F2}");
                        }
                    }
                    catch (Exception e)
                    {
                        // Log the error
                        Console.WriteLine($"ERROR parsing a file: {e.Message}");
                        continue;
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

            if (args.Length == 0)
            {
                // If no arguments are provided, run export all difficulties
                inputData.CalculateAndExportAllDifficulties(Constants.DATA_DIR);
            }
            else
            {
                // If any argument is provided, run filtered difficulties
                string filterBy = string.Join(" ", args);
                inputData.CalculateAndExportFilteredDifficulties(filterBy, Constants.DATA_DIR);
            }
        }
    }
}