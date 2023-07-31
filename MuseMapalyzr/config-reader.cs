using YamlDotNet.Serialization;
namespace MuseMapalyzr
{
    public class ConfigReader
    {
        // private static readonly string ConfigFilePath = "config/config.yaml";

        public static string GetConfigPath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string configFilePath = Path.Combine(currentDirectory, "config", "config.yaml");
            return configFilePath;
        }

        public static dynamic GetConfig()
        {
            string yamlContent = File.ReadAllText(GetConfigPath());
            var deserializer = new DeserializerBuilder().Build();
            var config = deserializer.Deserialize<dynamic>(yamlContent);
            return config;
        }
    }
}