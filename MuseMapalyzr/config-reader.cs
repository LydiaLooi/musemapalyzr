using YamlDotNet.Serialization;
namespace MuseMapalyzr
{
    public class ConfigReader
    {
        private static readonly string ConfigFilePath = "config/config.yaml";

        public static dynamic GetConfig()
        {
            string yamlContent = File.ReadAllText(ConfigFilePath);
            var deserializer = new DeserializerBuilder().Build();
            var config = deserializer.Deserialize<dynamic>(yamlContent);
            return config;
        }
    }
}