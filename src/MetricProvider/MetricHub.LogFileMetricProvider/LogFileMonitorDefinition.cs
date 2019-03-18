using Newtonsoft.Json;

namespace MetricHub.MetricProvider
{
    public class LogFileMonitorDefinition
    {
        [JsonProperty("DirectoryPath")]
        public string DirectoryPath { get; set; }

        [JsonProperty("Filter")]
        public string Filter { get; set; }

        [JsonProperty("IncludeSubdirectories")]
        public bool IncludeSubdirectories { get; set; } = false;
    }
}
