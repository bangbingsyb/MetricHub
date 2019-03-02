using Microsoft.Diagnostics.Tracing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MetricHub.MetricProvider
{
    public class EtwProviderDefinition
    {
        [JsonProperty("ProviderName")]
        public string ProviderName { get; set; }

        [JsonProperty("ProviderGuid")]
        public string ProviderGuid { get; set; }

        [JsonProperty("Flags")]
        public ulong Flags { get; set; }

        [JsonProperty("Level")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TraceEventLevel Level { get; set; }
    }
}
