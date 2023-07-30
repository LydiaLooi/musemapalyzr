
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MuseMapalyzr
{




    public class WeightingResults // Named Tuple "Weighting" in python code
    {
        public int Weighting;
        public int Difficulty;
        public int WeightedDifficulty;
    }

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

        public WeightingResults CalculateDifficulty(List<Note> notes, StreamWriter outfile, int SampleRate)
        {
            // TODO: Implement this
            WeightingResults weightResults = new();

            // Temp
            weightResults.Weighting = 1;
            weightResults.Difficulty = 1;
            weightResults.WeightedDifficulty = 1;

            return weightResults;
        }

        public void ProcessDifficulties(string[] files)
        {

            DateTime now = DateTime.Now;
            string outputFile = Path.Combine(Constants.outputDir, $"{now.ToString("yyyy-MM-dd_HH-mm-ss")}_difficulties_data.txt");

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
                            WeightingResults weightResults = CalculateDifficulty(mMap.Notes, outfile, mMap.SampleRate);
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
                inputData.CalculateAndExportAllDifficulties(Constants.dataDir);
            }
            else
            {
                // If any argument is provided, run filtered difficulties
                string filterBy = string.Join(" ", args);
                inputData.CalculateAndExportFilteredDifficulties(filterBy, Constants.dataDir);
            }
        }
    }
}