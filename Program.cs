
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MuseMapalyzr
{

    public static class Constants
    {
        public static string dataDir = "data";
        public static string outputDir = "difficulty_exports";
        public static string mapOutputs = "map_outputs";
    }

    public class Note
    {
        public int Lane { get; set; }
        public int SampleTime { get; set; }

        public Note(int lane, int sampleTime)
        {
            this.Lane = lane;
            this.SampleTime = sampleTime;
        }

    }

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
            // foreach (string file in allFiles)
            // {
            //     Console.WriteLine(file);
            // }
            ProcessDifficulties(allFiles);
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
                    break;
                }
            }
        }

    }

    class MainProgram
    {

        static void Main(string[] args)
        {
            InputData inputData = new();
            inputData.CalculateAndExportAllDifficulties(Constants.dataDir);

        }
    }
}