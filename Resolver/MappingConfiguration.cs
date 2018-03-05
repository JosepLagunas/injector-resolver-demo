using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Yuki.Core.Resolver.Countries;

namespace Yuki.Core.Resolver
{
    abstract class Mapping
    {
        [JsonProperty(PropertyName = "interface")]
        public string Interface { get; set; }
        [JsonProperty(PropertyName = "multi-implementation")]
        public bool MultiImplementation { get; set; }
    }
    class SingleMapping : Mapping
    {
        [JsonProperty(PropertyName = "implementation")]
        public string Implementation { get; set; }
    }
    class MultiMapping : Mapping
    {
        [JsonProperty(PropertyName = "implementations")]
        public IEnumerable<SpecificImplementation> Implementations { get; set; }
    }
    class SpecificImplementation
    {
        [JsonProperty(PropertyName = "discriminator")]
        public Country Discriminator { get; set; }
        [JsonProperty(PropertyName = "implementation")]
        public string Implementation { get; set; }
    }
    class MappingConfiguration
    {
        [JsonProperty(PropertyName = "assemblies-files-paths")]
        public IEnumerable<string> AssembliesFiles { get; set; }
        [JsonProperty(PropertyName = "mappings")]
        public IEnumerable<Mapping> Mappings { get; set; }


        public static MappingConfiguration LoadMappingConfiguration(string mappingConfigFilePath)
        {
            using (StreamReader r = new StreamReader(mappingConfigFilePath))
            {
                string json = r.ReadToEnd();

                JsonConverter[] converters = { new ResolverMappingJsonConverter() };
                MappingConfiguration mappingConfiguration =
                    JsonConvert.DeserializeObject<MappingConfiguration>(json,
                    new JsonSerializerSettings() { Converters = converters });

                return mappingConfiguration;
            }
        }
    }
}
