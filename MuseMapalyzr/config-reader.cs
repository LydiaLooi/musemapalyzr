using YamlDotNet.Serialization;
namespace MuseMapalyzr
{
    public class ConfigReader
    {
        public static string GetConfigPath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(currentDirectory, "config", "config.yaml");
            return configFilePath;
        }

        public static MuseMapalyzrConfig GetConfig()
        {
            string yamlContent = File.ReadAllText(GetConfigPath());
            var deserializer = new DeserializerBuilder().Build();
            MuseMapalyzrConfig config = deserializer.Deserialize<MuseMapalyzrConfig>(yamlContent);
            return config;
        }

        public class MuseMapalyzrConfig
        {
            public int PatternToleranceMs { get; set; }
            public int SegmentToleranceMs { get; set; }
            public double GetPatternWeightingTopPercentage { get; set; }
            public double GetPatternWeightingTopWeight { get; set; }
            public double GetPatternWeightingBottomWeight { get; set; }
            public int SampleWindowSecs { get; set; }
            public int MovingAvgWindow { get; set; }
            public int ShortIntervalNps { get; set; }
            public int MedIntervalNps { get; set; }
            public double LongIntervalNps { get; set; }
            public double DefaultVariationWeighting { get; set; }
            public double DefaultPatternWeighting { get; set; }
            public double ShortIntDebuff { get; set; }
            public double MedIntDebuff { get; set; }
            public double LongIntDebuff { get; set; }
            public double ExtraIntEndDebuff { get; set; }
            public double OtherSwitchMultiplier { get; set; }
            public double OtherShortIntMultiplier { get; set; }
            public double OtherMedIntMultiplier { get; set; }
            public double OtherLongIntMultiplier { get; set; }
            public double NothingButTheoryLowBound { get; set; }
            public double NothingButTheoryUpBound { get; set; }
            public double NothingButTheoryLowClamp { get; set; }
            public double NothingButTheoryUpClamp { get; set; }
            public double VaryingStreamsLowBound { get; set; }
            public double VaryingStreamsUpBound { get; set; }
            public double VaryingStreamsLowClamp { get; set; }
            public double VaryingStreamsUpClamp { get; set; }
            public double ZigZagLowBound { get; set; }
            public double ZigZagUpBound { get; set; }
            public double ZigZagLowClamp { get; set; }
            public double ZigZagUpClamp { get; set; }
            public double EvenCircleLowBound { get; set; }
            public double EvenCircleUpBound { get; set; }
            public double SkewedCircleLowBound { get; set; }
            public double SkewedCircleUpBound { get; set; }
            public double StreamLowBound { get; set; }
            public double StreamUpBound { get; set; }
            public double StreamLowClamp { get; set; }
            public double StreamUpClamp { get; set; }
            public double PatternStreamLengthLowBound { get; set; }
            public double PatternStreamLengthUpBound { get; set; }
            public double PatternStreamLengthLowClamp { get; set; }
            public double PatternStreamLengthUpClamp { get; set; }
            public double ZigZagLengthLowBound { get; set; }
            public double ZigZagLengthUpBound { get; set; }
            public double ZigZagLengthLowClamp { get; set; }
            public double ZigZagLengthUpClamp { get; set; }
            public double ZigZagLengthNpsThreshold { get; set; }
            public double FourStackLowBound { get; set; }
            public double FourStackUpBound { get; set; }
            public double FourStackLowClamp { get; set; }
            public double FourStackUpClamp { get; set; }
            public double ThreeStackLowBound { get; set; }
            public double ThreeStackUpBound { get; set; }
            public double ThreeStackLowClamp { get; set; }
            public double ThreeStackUpClamp { get; set; }
            public double TwoStackLowBound { get; set; }
            public double TwoStackUpBound { get; set; }
            public double TwoStackLowClamp { get; set; }
            public double TwoStackUpClamp { get; set; }
            public double VaryingStacksLowBound { get; set; }
            public double VaryingStacksUpBound { get; set; }
            public double VaryingStacksLowClamp { get; set; }
            public double VaryingStacksUpClamp { get; set; }
        }

    }
}