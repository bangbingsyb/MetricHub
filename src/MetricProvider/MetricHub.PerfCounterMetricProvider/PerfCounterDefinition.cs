using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MetricHub.MetricProvider
{
    public class PerfCounterDefinition
    {

        [JsonProperty("CategoryName")]
        public string CategoryName { get; set; }

        [JsonProperty("CounterName")]
        public string CounterName { get; set; }

        [JsonProperty("InstanceName")]
        public string InstanceName { get; set; }
    }
}
