using YamlDotNet.Serialization;
namespace MuseMapalyzr
{
    public class ConfigReader
    {
        private static MuseMapalyzrConfig? _rankedConfig;
        private static MuseMapalyzrConfig? _unrankedConfig;

        public static string GetConfigPath(bool ranked = true)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath;
            if (ranked)
            {
                configFilePath = Path.Combine(currentDirectory, "config", "config.yaml");
            }
            else
            {
                configFilePath = Path.Combine(currentDirectory, "config", "unranked_config.yaml");
            }

            return configFilePath;
        }

        private static MuseMapalyzrConfig DeserializeConfig(bool ranked)
        {
            string yamlContent = File.ReadAllText(GetConfigPath(ranked));
            var deserializer = new DeserializerBuilder().Build();
            MuseMapalyzrConfig config = deserializer.Deserialize<MuseMapalyzrConfig>(yamlContent);
            return config;
        }

        //// This is the method that is needed when copying over the MuseSwipr
        // private static MuseMapalyzrConfig DeserializeConfig(bool ranked)
        // {
        //     string configFile;
        //     if (ranked)
        //     {
        //         configFile = "musemapalyzrconfig";
        //     }
        //     else
        //     {
        //         configFile = "musemapalyzrunrankedconfig";
        //     }
        //     string yamlContent = Resources.Load<TextAsset>(configFile).text;
        //     var input = new StringReader(yamlContent);
        //     var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
        //     var config = deserializer.Deserialize<MuseMapalyzrConfig>(input);
        //     return config;
        // }


        public static MuseMapalyzrConfig GetConfig()
        {
            _rankedConfig ??= DeserializeConfig(true);
            return _rankedConfig;
        }

        public static MuseMapalyzrConfig GetUnrankedConfig()
        {
            _unrankedConfig ??= DeserializeConfig(false);
            return _unrankedConfig;
        }

        public class MuseMapalyzrConfig
        {
            public int NormalSizedMapThreshold { get; set; }
            public int HardestSeconds { get; set; }
            public double Arbitrary90PercentThreshold { get; set; }
            public double RankedPenaltyProportion { get; set; }
            public double CeilingProportion { get; set; }

            public double DensitySingleStreamNPSCap { get; set; }
            public double DensityFourStackNPSCap { get; set; }
            public double DensityThreeStackNPSCap { get; set; }
            public double DensityTwoStackNPSCap { get; set; }
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
            public double ZigZagBaseMultiplier { get; set; }
            public double ZigZagLowBound { get; set; }
            public double ZigZagUpBound { get; set; }
            public double ZigZagUnsusGradient { get; set; }
            public double ZigZagSusBeginningNPS { get; set; }
            public double ZigZagUnsusStartingMultiplier { get; set; }
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
            public double SusZigZagLengthLowBound { get; set; }
            public double SusZigZagLengthUpBound { get; set; }
            public double SusZigZagLengthLowClamp { get; set; }
            public double SusZigZagLengthUpClamp { get; set; }
            public double UnsusZigZagLengthLowBound { get; set; }
            public double UnsusZigZagLengthUpBound { get; set; }
            public double UnsusZigZagLengthLowClamp { get; set; }
            public double UnsusZigZagLengthUpClamp { get; set; }
            public double UnsusZigZagThresholdNPS { get; set; }
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
            public double VaryingStackMultiplier { get; set; }
        }

    }
}